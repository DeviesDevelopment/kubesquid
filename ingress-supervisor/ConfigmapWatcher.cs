using ingress_supervisor;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DefaultNamespace;

public class ConfigmapWatcher : BackgroundService
{
    private readonly Kubernetes _client;
    private readonly KubernetesWrapper _kubernetesWrapper;
    private readonly string _targetNamespace;
    private readonly ILogger<ConfigmapWatcher> _logger;

    public ConfigmapWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper, ILogger<ConfigmapWatcher> logger)
    {
        _client = client;
        _kubernetesWrapper = kubernetesWrapper;
        _logger = logger;
        _targetNamespace = config.Namespace;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WatchConfigmaps();
        }
    }
    private async Task WatchConfigmaps()
    {
        var configMapResp = _client.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(_targetNamespace, fieldSelector: "metadata.name=kubesquid", watch: true);
        _logger.LogInformation("Starting to watch the config map");
        await foreach (var (type, configMap) in configMapResp.WatchAsync<V1ConfigMap, V1ConfigMapList>())
        {
            _logger.LogInformation("Got Configmap Event: {} for configmap: {}", type, configMap.Metadata.Name);
        }
    }
}
