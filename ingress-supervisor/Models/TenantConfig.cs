using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using k8s.Models;

namespace ingress_supervisor.Models;

public class TenantConfig
{
    public const int IngressNameMaxLength = 253;
    public string ServiceName { get; set; }

    public string InstanceId { get; set; }

    public string HostName { get; set; }

    public string Path { get; set; }

    public string GetIngressName()
    {
        var source = Encoding.ASCII.GetBytes($"{ServiceName}{InstanceId}{HostName}{Path}");
        var hash = BitConverter.ToString(SHA1.Create().ComputeHash(source)).Replace("-", string.Empty).ToLower();
        var remainingLength = IngressNameMaxLength - hash.Length;
        var prefix = $"{ServiceName}-{InstanceId}";
        return prefix.Length <= remainingLength - 1? $"{prefix}-{hash}" : $"{prefix.Substring(0, remainingLength)}-{hash}";
    }

    public override string ToString()
    {
        return $"ServiceName: {ServiceName}, InstanceId: {InstanceId}, HostName: {HostName}, Path: {Path}";
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
