using System.Collections.ObjectModel;
using k8s;
using k8s.Models;

namespace ingress_supervisor;

public class KubernetesClient
{
    
public async void CreateIngress(Kubernetes client, string targetNamespace)
{
    var ingress = new V1Ingress()
    {
        Kind = "Ingress",
        Metadata = new V1ObjectMeta()
        {
            NamespaceProperty = targetNamespace,
            Name = "kalleanka",
            Labels = new Dictionary<string, string>()
            {
                { "autocreated", "Sant" }
            },
            Annotations = new Dictionary<string, string>()
            {
                {"kubernetes.io/ingress.class", "nginx"},
                {"nginx.ingress.kubernetes.io/rewrite-target", "/$1"},
                {"nginx.ingress.kubernetes.io/use-regex", "true"},
                {
                    "nginx.ingress.kubernetes.io/configuration-snippet", @$"
                        proxy_set_header InstanceId 42;
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
                    Host = "baloo.devies.com",
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
                                        Name = "my-release-whoami",
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
}}