using AutoMapper.Internal;
using ingress_supervisor.Models;
using k8s.Models;

namespace ingress_supervisor;

public static class MyExtensions
{
    public static bool HasMatchingServiceConfigExtension(this V1Ingress ingress, IList<TenantConfig> squidConfig)
    {
        if (!"kubesquid".Equals(ingress.Metadata.Labels.GetOrDefault("app.kubernetes.io/created-by")))
        {
            return false;
        }

        var matchingServiceConfigs = squidConfig
            .Where(serviceConfig => serviceConfig.HostName.Equals(ingress.Spec.Rules.First().Host))
            .Where(serviceConfig => serviceConfig.Path.Equals(ingress.Spec.Rules.First().Http.Paths.First().Path))
            .Where(serviceConfig =>
            {
                if (!ingress.Metadata.Annotations.ContainsKey("nginx.ingress.kubernetes.io/configuration-snippet"))
                    return false;
                return ingress.Metadata.Annotations["nginx.ingress.kubernetes.io/configuration-snippet"].Contains(serviceConfig.InstanceId);
            });

        return matchingServiceConfigs.Any();
    }
}
