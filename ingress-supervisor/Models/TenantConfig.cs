namespace ingress_supervisor.Models;

public class TenantConfig
{
    public string ServiceName { get; set; }
    public string InstanceId { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public string Path { get; set; }

    public string GetIngressName() => $"{ServiceName}-{InstanceId}-ingress";

    public override string ToString()
    {
        return $"ServiceName: {ServiceName}, InstanceId: {InstanceId}, HostName: {HostName}, Port: {Port}, Path: {Path}";
    }
}
