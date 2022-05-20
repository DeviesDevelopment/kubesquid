using AutoMapper.Internal;
using ingress_supervisor.Models;
using k8s.Models;

namespace ingress_supervisor;

public class Logic
{

    public bool ServiceHasIngress(IList<V1Ingress> ingresses, TenantConfig serviceConfig)
    {
        if (!ingresses.Any())
        {
            return false;
        }

        var matchingIngresses = ingresses
                .Where(ingress => "kubesquid".Equals(ingress.Metadata.Labels.GetOrDefault("app.kubernetes.io/created-by")))
                .Where(ingress => ingress.Metadata.Name.Equals(serviceConfig.GetIngressName()));
        return matchingIngresses.Any();
    }

    public bool IngressHasServiceConfig(V1Ingress ingress, IList<TenantConfig> squidConfig)
    {
        if (!"kubesquid".Equals(ingress.Metadata.Labels.GetOrDefault("app.kubernetes.io/created-by")))
        {
            return false;
        }

        var matchingServiceConfigs = squidConfig
            .Where(serviceConfig => serviceConfig.GetIngressName().Equals(ingress.Metadata.Name));
        return matchingServiceConfigs.Any();
    }
}
