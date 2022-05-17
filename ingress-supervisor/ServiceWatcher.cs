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
        //TODO: Stop watching service list when shutting down.
        /*stoppingToken.Register(() =>
        {

        })
        */

        while (!stoppingToken.IsCancellationRequested)
        {
            await WatchServices();
        }
    }
    private async Task WatchServices()
    {
        var servicesListResp = _client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(_targetNamespace, watch: true);
        Console.WriteLine("Staring to watch services");
        await foreach (var (type, service) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
        {
            Console.WriteLine($"Got Service Event of Type: {type} for Service: {service.Metadata.Name}");
            switch (type)
            {
                case WatchEventType.Added:
                    if (service.Metadata?.Annotations?.ContainsKey("squid") == true)
                    {
                        var squidConfig = await _kubernetesWrapper.GetSquidConfig();
                        var serviceConfigs = squidConfig
                            .Where(config => config.ServiceName.Equals(service.Metadata.Name))
                            .ToList();
                        // TODO: Check if ingress already exist, for each service config
                        foreach (var serviceConfig in serviceConfigs)
                        {
                            _kubernetesWrapper.CreateIngress(serviceConfig);
                            Console.WriteLine($"Created ingress for service: {serviceConfig.ServiceName}");
                        }
                    }

                    break;
                default:
                    break;
            }
        }
    }
}

