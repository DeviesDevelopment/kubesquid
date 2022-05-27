using System.Collections.ObjectModel;
using ingress_supervisor;
using ingress_supervisor.Models;
using k8s.Models;

namespace unit_tests;

public class LogicTests
{
    private Logic _logic = new Logic();

    [Fact]
    public void ServiceHasMatchingIngress_Matching()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/customer-a");
        var ingresses = CreateIngresses("666", "mowgli.devies.com", "test-service", 80, "/customer-a");
        Assert.True(_logic.ServiceHasMatchingIngress(ingresses, serviceConfig));
    }

    [Fact]
    public void ServiceHasMatchingIngress_HostMismatch()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/customer-a");
        var ingresses = CreateIngresses("666", "baloo.devies.com", "test-service", 80, "/customer-a");
        Assert.False(_logic.ServiceHasMatchingIngress(ingresses, serviceConfig));
    }

    [Fact]
    public void ServiceHasMatchingIngress_PathMismatch()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/some-new-path");
        var ingresses = CreateIngresses("666", "mowgli.devies.com", "test-service", 80, "/customer-a");
        Assert.False(_logic.ServiceHasMatchingIngress(ingresses, serviceConfig));
    }

    [Fact]
    public void ServiceHasMatchingIngress_InstanceIdMismatch()
    {
        var serviceConfig = CreateServiceConfig("test-service", "some-new-instance-id", "mowgli.devies.com", 80, "/customer-a");
        var ingresses = CreateIngresses("666", "mowgli.devies.com", "test-service", 80, "/customer-a");
        Assert.False(_logic.ServiceHasMatchingIngress(ingresses, serviceConfig));
    }

    [Fact]
    public void ServiceHasMatchingIngresses_NoIngressesExists()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "baloo.devies.com", 80, "/customer-a");
        Assert.False(_logic.ServiceHasMatchingIngress(new List<V1Ingress>(), serviceConfig));
    }

    [Fact]
    public void IngressHasMatchingServiceConfig_Matching()
    {
        var serviceConfig = CreateServiceConfig("test-service", "666", "baloo.devies.com", 80, "/customer-a");
        var ingresses = CreateIngresses("666", "baloo.devies.com", "test-service", 80, "/customer-a");
        Assert.True(_logic.IngressHasMatchingServiceConfig(ingresses.First(), new List<TenantConfig> { serviceConfig }));
    }

    [Fact]
    public void IngressHasMatchingServiceConfig_NoServiceExist()
    {
        var ingresses = CreateIngresses("666", "baloo.devies.com", "test-service", 80, "/customer-a");
        Assert.False(_logic.IngressHasMatchingServiceConfig(ingresses.First(), new List<TenantConfig>()));
    }

    [Fact]
    public void IngressHasMatchingServiceConfig_HostMismatch()
    {
        var ingress = CreateIngresses("666", "baloo.devies.com", "test-service", 80, "/customer-a").First();
        var serviceConfigs = new List<TenantConfig> { CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/customer-a") };
        Assert.False(_logic.IngressHasMatchingServiceConfig(ingress, serviceConfigs));
    }

    [Fact]
    public void IngressHasMatchingServiceConfig_PathMismatch()
    {
        var ingress = CreateIngresses("666", "mowgli.devies.com", "test-service", 80, "/some/other/path").First();
        var serviceConfigs = new List<TenantConfig> { CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/customer-a") };
        Assert.False(_logic.IngressHasMatchingServiceConfig(ingress, serviceConfigs));
    }

    [Fact]
    public void IngressHasMatchingServiceConfig_InstanceIdMismatch()
    {
        var ingress = CreateIngresses("999", "mowgli.devies.com", "test-service", 80, "/customer-a").First();
        var serviceConfigs = new List<TenantConfig> { CreateServiceConfig("test-service", "666", "mowgli.devies.com", 80, "/customer-a") };
        Assert.False(_logic.IngressHasMatchingServiceConfig(ingress, serviceConfigs));
    }

    private List<V1Ingress> CreateIngresses(string instanceId, string host, string serviceName, int port, string path)
    {
        return new List<V1Ingress>()
        {
            new V1Ingress()
            {
                Kind = "Ingress",
                Metadata = new V1ObjectMeta()
                {
                    NamespaceProperty = "default",
                    Name = new TenantConfig
                    {
                        HostName = host,
                        InstanceId = instanceId,
                        Path = path,
                        ServiceName = serviceName
                    }.GetIngressName(),
                    Labels = new Dictionary<string, string>()
                    {
                        { "autocreated", "true" }, // TODO: Yeet me
                        { "app.kubernetes.io/created-by", "kubesquid" }
                    },
                    Annotations = new Dictionary<string, string>()
                    {
                        { "kubernetes.io/ingress.class", "nginx" },
                        { "nginx.ingress.kubernetes.io/rewrite-target", "/$1" },
                        { "nginx.ingress.kubernetes.io/use-regex", "true" },
                        {
                            $"nginx.ingress.kubernetes.io/configuration-snippet", @$"
                        proxy_set_header InstanceId {instanceId};
                    "
                        },
                        { "nginx.ingress.kubernetes.io/proxy-body-size", "600m" },
                        { "nginx.org/client-max-body-size", "600m" }
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
                                                Port = new V1ServiceBackendPort() { Number = port }
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
            Path = path
        };
    }
}
