using System.Collections.ObjectModel;
using ingress_supervisor;
using ingress_supervisor.Models;
using k8s.Models;

namespace unit_tests;

public class LogicTests
{
    private Logic _logic = new Logic();

    [Fact]
    public void ServiceHasIngress_IngressExists()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "baloo.devies.com", 80, "/customer-a");
        var ingresses = CreateIngresses(serviceConfig.GetIngressName(), "666", "baloo.devies.com", "test-service", 80, "/customer-a");
        Assert.True(_logic.ServiceHasIngress(ingresses, serviceConfig));
    }

    [Fact]
    public void ServiceHasIngresses_NoIngressesExists()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "baloo.devies.com", 80, "/customer-a");
        Assert.False(_logic.ServiceHasIngress(new List<V1Ingress>(), serviceConfig));
    }


    private List<V1Ingress> CreateIngresses(string name, string instanceId, string host, string serviceName, int port, string path)
    {
        return new List<V1Ingress>()
        {
            new V1Ingress()
            {
            Kind = "Ingress",
            Metadata = new V1ObjectMeta()
            {
                NamespaceProperty = "default",
                Name = name,
                Labels = new Dictionary<string, string>()
            {
                { "autocreated", "true" }, // TODO: Yeet me
                { "app.kubernetes.io/created-by", "kubesquid" }
            },
                Annotations = new Dictionary<string, string>()
            {
                {"kubernetes.io/ingress.class", "nginx"},
                {"nginx.ingress.kubernetes.io/rewrite-target", "/$1"},
                {"nginx.ingress.kubernetes.io/use-regex", "true"},
                {
                    $"nginx.ingress.kubernetes.io/configuration-snippet", @$"
                        proxy_set_header InstanceId {instanceId};
                    "
                },
                {"nginx.ingress.kubernetes.io/proxy-body-size", "600m"},
                {"nginx.org/client-max-body-size", "600m"}
            }
            },
            Spec = new V1IngressSpec()
            {
                Rules = new List<V1IngressRule>()
            {
                new V1IngressRule()
                {
                    Host = host,
                    Http = new V1HTTPIngressRuleValue()
                    {
                        Paths = new Collection<V1HTTPIngressPath>()
                        {
                            new V1HTTPIngressPath()
                            {
                                Path = path,
                                PathType = "ImplementationSpecific",
                                Backend = new V1IngressBackend()
                                {
                                    Service = new V1IngressServiceBackend()
                                    {
                                        Name = serviceName,
                                        Port = new V1ServiceBackendPort()
                                        {
                                            Number = port
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                }
            }
            }
        };
    }

    private TenantConfig CreateServiceConfig(string serviceName, string instanceId, string hostname, int port, string path)
    {
        return new TenantConfig()
        {
            ServiceName = serviceName,
            InstanceId = instanceId,
            HostName = hostname,
            Port = port,
            Path = path
        };
    }
}