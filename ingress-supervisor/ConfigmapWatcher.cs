using ingress_supervisor;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;

namespace DefaultNamespace;

public class ConfigmapWatcher : BackgroundService
{
    private readonly Kubernetes _client;
    private readonly KubernetesWrapper _kubernetesWrapper;
    private readonly string _targetNamespace;

    public ConfigmapWatcher(Kubernetes client, KubernetesClientConfiguration config, KubernetesWrapper kubernetesWrapper)
    {
        _client = client;
        _kubernetesWrapper = kubernetesWrapper;
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
        Console.WriteLine("Starting to watch the config map");
        await foreach (var (type, configMap) in configMapResp.WatchAsync<V1ConfigMap, V1ConfigMapList>())
        {
            Console.WriteLine($"Got Configmap Event: {type} for configmap: {configMap.Metadata.Name}");
        }
    }
}
