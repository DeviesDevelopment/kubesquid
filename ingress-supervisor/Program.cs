using k8s;
using k8s.Models;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var servicesListResp = client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync("default", watch: true);
await foreach (var (type, item) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
{
    Console.WriteLine("==on watch event==");
    Console.WriteLine(type);
    Console.WriteLine(item.Metadata.Name);
    Console.WriteLine("==on watch event==");
}   

