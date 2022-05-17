using DefaultNamespace;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<IHostedService>(x => new ServiceWatcher(client, config.Namespace));
});

await builder.RunConsoleAsync();

/*
var servicesListResp = client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(config.Namespace, watch: true);
var configMapResp = client.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(config.Namespace, watch: true);
var kubernetesClient = new KubernetesWrapper();

async Task WatchServices() 
{ 
    Console.WriteLine("Staring to watch services");
    await foreach (var (type, service) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
    {
        switch (type)
        {
            case WatchEventType.Added:
                
                break;
            default:
                break;
        }
        Console.WriteLine("==Watching Service Events==");
        Console.WriteLine(type);
        Console.WriteLine(service.Metadata.Name);
        Console.WriteLine("==Watching Service Events==");
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

        var configMapContent = await client.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(configMap.Metadata.Name, config.Namespace);
        foreach (var key in configMapContent.Body.Data.Keys)
        {
            Console.WriteLine($"{key}: {configMapContent.Body.Data[key]}");
        }
    }
}
try
{
    Task.WaitAll(new[] { WatchServices(), WatchConfigMap() });
}
catch (AggregateException e)
{ 
    Console.WriteLine("An exception occured in one of our background tasks");
    Console.WriteLine(e.Message); 
}
*/

Console.WriteLine("Bye, World!");
