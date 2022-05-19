using ingress_supervisor;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DefaultNamespace;

public class ServiceWatcher : BackgroundService
{
    private readonly Kubernetes _client;
    private readonly string _targetNamespace;
    private readonly KubernetesWrapper _kubernetesWrapper;
    private readonly Logic _logic;
    private readonly ILogger<ServiceWatcher> _logger;

    public ServiceWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper, Logic logic, ILogger<ServiceWatcher> logger)
    {
        _client = client;
        _targetNamespace = config.Namespace;
        _kubernetesWrapper = kubernetesWrapper;
        _logic = logic;
        _logger = logger;
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
        _logger.LogInformation("Starting to watch services");
        await foreach (var (type, service) in servicesListResp.WatchAsync<V1Service, V1ServiceList>())
        {
            _logger.LogInformation("Got Service Event of Type: {} for Service: {}", type, service.Metadata.Name);
            if (service.Metadata?.Annotations?.ContainsKey("squid") != true)
            {
                _logger.LogInformation("Ignoring service {} due to missing squid annotation", service.Metadata.Name);
                continue;
            }

            var squidConfig = await _kubernetesWrapper.GetSquidConfig();
            var serviceConfigs = squidConfig
                .Where(config => config.ServiceName.Equals(service.Metadata.Name))
                .ToList();
            var allIngresses = await _kubernetesWrapper.GetIngresses();

            switch (type)
            {
                case WatchEventType.Added:
                    foreach (var serviceConfig in serviceConfigs)
                    {
                        if (!_logic.ServiceHasIngress(allIngresses, serviceConfig))
                        {
                            await _kubernetesWrapper.CreateIngress(serviceConfig);
                            _logger.LogInformation("Created ingress for service: {}", service.Metadata.Name);
                        }
                    }
                    break;
                case WatchEventType.Deleted:
                    foreach (var serviceConfig in serviceConfigs)
                    {
                        if (_logic.ServiceHasIngress(allIngresses, serviceConfig))
                        {
                            await _kubernetesWrapper.DeleteIngress(serviceConfig);
                            _logger.LogInformation("Deleted ingress for service: {}", serviceConfig.ServiceName);
                        }
                    }
                    break;
            }
        }
    }
}
