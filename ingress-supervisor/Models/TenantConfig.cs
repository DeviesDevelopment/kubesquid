using System.Text.Json;
using k8s.Models;

namespace ingress_supervisor.Models;

public class TenantConfig
{
    public string ServiceName { get; set; }

    public string InstanceId { get; set; }

    public string HostName { get; set; }

    // TODO: Do we need to have port in config?
    public int Port { get; set; }

    public string Path { get; set; }

    public string GetIngressName() => $"{ServiceName}-{InstanceId}-ingress-{Guid.NewGuid().ToString("n").Substring(0, 8)}";

    public override string ToString()
    {
        return $"ServiceName: {ServiceName}, InstanceId: {InstanceId}, HostName: {HostName}, Port: {Port}, Path: {Path}";
    }

    // TODO: Handle when deserialize not possible
    public static List<TenantConfig?> FromConfigMap(V1ConfigMap configMap)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return configMap.Data
            .Select(pair => JsonSerializer.Deserialize<TenantConfig>(pair.Value, options))
            .ToList();
    }
}
