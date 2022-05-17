using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;

namespace DefaultNamespace;

public class ConfigmapWatcher : BackgroundService
{
    private readonly Kubernetes _client;
    private readonly string _targetNamespace;

    public ConfigmapWatcher(Kubernetes client, KubernetesClientConfiguration config)
    {
        _client = client;
        _targetNamespace = config.Namespace;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WatchConfigmaps();
        }
        // TODO: Clean up
    }
    private async Task WatchConfigmaps()
    {
        var configMapResp = _client.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(_targetNamespace, watch: true);
        Console.WriteLine("Starting to watch the config map");
        await foreach (var (type, configMap) in configMapResp.WatchAsync<V1ConfigMap, V1ConfigMapList>())
        {
            Console.WriteLine("==Watching ConfigMap Events==");
            Console.WriteLine(type);
            Console.WriteLine(configMap.Metadata.Name);
            Console.WriteLine("==Watching ConfigMap Events==");

            var configMapContent = await _client.CoreV1.ReadNamespacedConfigMapWithHttpMessagesAsync(configMap.Metadata.Name, _targetNamespace);
            foreach (var key in configMapContent.Body.Data.Keys)
            {
                Console.WriteLine($"{key}: {configMapContent.Body.Data[key]}");
            }
        }
    }
}
