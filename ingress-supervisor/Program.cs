using k8s;
using k8s.Models;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);
var servicesListResp = client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(config.Namespace, watch: true);
var configMapResp = client.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(config.Namespace, watch: true);

async Task WatchServices() 
{ 
    Console.WriteLine("Staring to watch services");
    await foreach (var (type, service) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
    {
        Console.WriteLine("==Watching Service Events==");
        Console.WriteLine(type);
        Console.WriteLine(service.Metadata.Name);
        Console.WriteLine("==Watching Service Events");
    }
}

async Task WatchConfigMap()
{
    Console.WriteLine("Starting to watch the config map");
    await foreach (var (type, configMap) in configMapResp.WatchAsync<V1ConfigMap, V1ConfigMapList>())
    {
        Console.WriteLine("==Watching ConfigMap Events==");
        Console.WriteLine(type);
        Console.WriteLine(configMap.Metadata.Name);
        Console.WriteLine("==Watching ConfigMap Events==");
    }
}

try
{
    Task.WaitAll(new[] { WatchServices(), WatchConfigMap() });
}
catch (Exception e)
{ 
    Console.WriteLine("EXCEPTION!!!");
    Console.WriteLine(e.Message); 
}


Console.WriteLine("Bye, World!");