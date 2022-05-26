using System.Security.Cryptography;
using System.Text;
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

    public string GetIngressName()
    {
        var source = Encoding.ASCII.GetBytes($"{ServiceName}{InstanceId}{HostName}{Port}{Path}");
        var hash = SHA256.Create().ComputeHash(source);
        return $"{ServiceName}-{InstanceId}-ingress-{BitConverter.ToString(hash).Replace("-", string.Empty)}"
            .Substring(0, 253)
            .ToLower();
    }

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
