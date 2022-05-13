using System.Collections.ObjectModel;
using k8s;
using k8s.Models;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var ingress = new V1Ingress()
{
    Kind = "Ingress",
    Metadata = new V1ObjectMeta()
    {
        NamespaceProperty = config.Namespace,
        Name = "kalleanka",
        Labels = new Dictionary<string, string>()
        {
            { "autocreated", "Sant" }
        },
        Annotations = new Dictionary<string, string>()
        {
            { "automatisktskapad", "true" }
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

var b = await client.CreateNamespacedIngressAsync(null, config.Namespace);

var servicesListResp = client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync("default", watch: true);
await foreach (var (type, item) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
{
    Console.WriteLine("==on watch event==");
    Console.WriteLine(type);
    Console.WriteLine(item.Metadata.Name);
    Console.WriteLine("==on watch event==");
}   

