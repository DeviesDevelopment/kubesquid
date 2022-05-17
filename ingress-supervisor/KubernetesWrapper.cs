using System.Collections.ObjectModel;
using k8s;
using k8s.Models;

namespace ingress_supervisor;

public class KubernetesWrapper
{

    public async void CreateIngress(Kubernetes client, string targetNamespace, string serviceName, string host, string instanceId)
    {
        var ingress = new V1Ingress()
        {
            Kind = "Ingress",
            Metadata = new V1ObjectMeta()
            {
                NamespaceProperty = targetNamespace,
                Name = $"{serviceName}-ingress",
                Labels = new Dictionary<string, string>()
            {
                { "autocreated", "true" }
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
                                Path = "/(.*)",
                                PathType = "ImplementationSpecific",
                                Backend = new V1IngressBackend()
                                {
                                    Service = new V1IngressServiceBackend()
                                    {
                                        Name = serviceName,
                                        Port = new V1ServiceBackendPort()
                                        {
                                            Number = 80
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

        var b = await client.CreateNamespacedIngressAsync(ingress, targetNamespace);
    }
}
