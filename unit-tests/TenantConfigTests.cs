using System.Text.RegularExpressions;
using ingress_supervisor.Models;

namespace unit_tests;

public class TenantConfigTests
{
    [Fact]
    public void TenantConfig_IngressNameShouldBeDeterministic()
    {
        var tenantConfig = new TenantConfig
        {
            HostName = "shere-khan.devies.com",
            Path = "/my-path",
            InstanceId = "666",
            Port = 80,
            ServiceName = "some-backend-service"
        };
        var tenantConfigWithIdenticalValues = new TenantConfig
        {
            HostName = "shere-khan.devies.com",
            Path = "/my-path",
            InstanceId = "666",
            Port = 80,
            ServiceName = "some-backend-service"
        };
        Assert.NotSame(tenantConfig, tenantConfigWithIdenticalValues);
        Assert.Equal(tenantConfig.GetIngressName(), tenantConfigWithIdenticalValues.GetIngressName());
    }

    [Fact] // See: https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#dns-subdomain-names
    public void TenantConfig_IngressNameShouldBeAValidDnsSubdomainName()
    {
        var ingressName = new TenantConfig()
        {
            HostName = "shere-khan.devies.com",
            Path = "/my-path",
            InstanceId = "666",
            Port = 80,
            ServiceName = "some-backend-service"
        }.GetIngressName();
        Assert.Matches(new Regex(@"^[a-z0-9][a-z0-9\-\.]*[a-z0-9]$"), ingressName);
        Assert.InRange(ingressName.Length, 1, TenantConfig.IngressNameMaxLength);
    }

    [Fact]
    public void TenantConfig_IngressNameShouldBeTruncatedButStillContainFullHash()
    {
        var veryLongServiceName = string.Concat(Enumerable.Repeat("some-backend-service", 13));
        var ingressName = new TenantConfig()
        {
            HostName = "shere-khan.devies.com",
            Path = "/my-path",
            InstanceId = "666",
            Port = 80,
            ServiceName = veryLongServiceName
        }.GetIngressName();
        var expectedHash = "b538347bddfcb2adc74477f5fdd1be5479b692a6";
        Assert.Contains(expectedHash, ingressName);
    }
}
