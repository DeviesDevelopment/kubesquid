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
    private readonly Logic _logic;

    public ServiceWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper, Logic logic)
    {
        _client = client;
        _targetNamespace = config.Namespace;
        _kubernetesWrapper = kubernetesWrapper;
        _logic = logic;
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
                        var allIngresses = await _kubernetesWrapper.GetIngresses();
                        foreach (var serviceConfig in serviceConfigs)
                        {
                            if (!_logic.ServiceHasIngress(allIngresses, serviceConfig))
                            {
                                _kubernetesWrapper.CreateIngress(serviceConfig);
                            }
                            Console.WriteLine($"Created ingress for service: {serviceConfig.ServiceName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ignoring service {service.Metadata.Name} due to missing squid annotation");
                    }

                    break;
                default:
                    break;
            }
        }
    }
}

