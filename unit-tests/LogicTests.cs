using ingress_supervisor;
using ingress_supervisor.Models;
using k8s.Models;

namespace unit_tests;

public class LogicTests
{
    private Logic _logic = new Logic();

    [Fact]
    public void ServiceHasIngresses_NoIngressesExists()
    {
        V1Service service = new V1Service()
        {
            Metadata = new V1ObjectMeta()
            {
                Annotations = new Dictionary<string, string>
                {
                    { "squid", "true" },
                },
                Name = "test-service",
                ClusterName = "my-cluster",
                Uid = "test-uid",
            }
        };

        var serviceConfigs = new List<TenantConfig>()
        {
            new TenantConfig()
            {
                ServiceName = "test-service",
                InstanceId = "666",
                HostName = "baloo.devies.com",
                Port = 80,
                Path = "/customer-a"
            }
        };
        Assert.False(_logic.ServiceHasIngress(service, new List<V1Ingress>(), serviceConfigs));
    }
}
