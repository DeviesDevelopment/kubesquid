using ingress_supervisor;
using ingress_supervisor.Models;
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
    private readonly ILogger<ServiceWatcher> _logger;

    public ServiceWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper, ILogger<ServiceWatcher> logger)
    {
        _client = client;
        _targetNamespace = config.Namespace;
        _kubernetesWrapper = kubernetesWrapper;
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

            // TODO: Check that configmap exists.
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
                        if (!serviceConfig.HasMatchingIngress(allIngresses))
                        {
                            await _kubernetesWrapper.CreateIngress(serviceConfig);
                            _logger.LogInformation("Created ingress for service: {}", service.Metadata.Name);
                        }
                    }

                    break;
                case WatchEventType.Deleted:
                    foreach (var serviceConfig in serviceConfigs)
                    {
                        if (serviceConfig.HasMatchingIngress(allIngresses))
                        {
                            await _kubernetesWrapper.DeleteIngress(serviceConfig.GetIngressName());
                            _logger.LogInformation("Deleted ingress for service: {}", serviceConfig.ServiceName);
                        }
                    }

                    break;
                case WatchEventType.Modified:
                    var allIngressesForService = await _kubernetesWrapper.FindAllIngressesForService(service.Metadata.Name);
                    foreach (var ingress in allIngressesForService)
                    {
                        var servicePort = service.Spec.Ports.First().Port;
                        var ingressPort = ingress.Spec.Rules.First().Http.Paths.First().Backend.Service.Port.Number;
                        if (!servicePort.Equals(ingressPort))
                        {
                            await _kubernetesWrapper.DeleteIngress(ingress.Metadata.Name);
                            await _kubernetesWrapper.CreateIngress(new TenantConfig()
                            {
                                ServiceName = service.Metadata.Name,
                                TenantId = ingress.Metadata.Labels["kubesquid-tenantid"],
                                HostName = ingress.Spec.Rules.First().Host,
                                Path = ingress.Spec.Rules.First().Http.Paths.First().Path
                            });
                        }
                    }

                    break;
            }
        }
    }
}
