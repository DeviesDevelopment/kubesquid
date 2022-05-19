using ingress_supervisor.Models;
using k8s.Models;

namespace ingress_supervisor;

public class Logic
{

    public bool ServiceHasIngress(V1Service service, List<V1Ingress> ingresses, List<TenantConfig> serviceConfigs)
    {
        if (!ingresses.Any() || !serviceConfigs.Any())
        {
            return false;
        }

        return true;
    }

}
