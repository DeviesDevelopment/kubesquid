using ingress_supervisor;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;

namespace DefaultNamespace;

public class ServiceWatcher : BackgroundService
{
    private readonly Kubernetes _client;
    private readonly string _targetNamespace;
    private readonly KubernetesWrapper _kubernetesWrapper;

    public ServiceWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper)
    {
        _client = client;
        _targetNamespace = config.Namespace;
        _kubernetesWrapper = kubernetesWrapper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WatchServices();
        }
        // TODO: Clean up
    }
    private async Task WatchServices()
    {
        var servicesListResp = _client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(_targetNamespace, watch: true);
        Console.WriteLine("Staring to watch services");
        await foreach (var (type, service) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
        {
            switch (type)
            {
                case WatchEventType.Added:
                    if (service.Metadata?.Annotations?.ContainsKey("squid") == true)
                    {
                    }
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
}

