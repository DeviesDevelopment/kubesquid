using System.Collections.ObjectModel;
using ingress_supervisor.Models;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace ingress_supervisor;

public class KubernetesWrapper
{
    private readonly ILogger<KubernetesWrapper> _logger;
    private const string ConfigMapName = "kubesquid";
    private readonly Kubernetes _client;
    private readonly string _targetNamespace;

    public KubernetesWrapper(Kubernetes client, KubernetesClientConfiguration config, ILogger<KubernetesWrapper> logger)
    {
        _client = client;
        _targetNamespace = config.Namespace;
        _logger = logger;
    }

    public async Task<IList<V1Ingress>> GetIngresses()
    {
        var ingresses = await _client.ListNamespacedIngressAsync(_targetNamespace);
        return ingresses.Items;
    }

    public async Task<IList<V1Ingress>> FindAllIngressesForService(string serviceName)
    {
        var ingresses = await GetIngresses();
        return ingresses.Where(i => i.Spec.Rules.Any(r => r.Http.Paths.Any(p => p.Backend.Service.Name.Equals(serviceName)))).ToList();
    }

    public async Task<List<TenantConfig?>> GetSquidConfig()
    {
        var configMap = await _client.ReadNamespacedConfigMapAsync(ConfigMapName, _targetNamespace);
        return TenantConfig.FromConfigMap(configMap);
    }

    public async Task CreateIngress(TenantConfig tenantConfig)
    {
        int servicePort = 0;
        try
        {
            var service = await _client.ReadNamespacedServiceAsync(tenantConfig.ServiceName, _targetNamespace);
            servicePort = service.Spec.Ports.First().Port;
        }
        catch (HttpOperationException e)
        {
            _logger.LogError("Failed to fetch port number for service {}, due to: {}", tenantConfig.ServiceName, e.Message);
            return;
        }

        var ingress = new V1Ingress()
        {
            Kind = "Ingress",
            Metadata = new V1ObjectMeta()
            {
                NamespaceProperty = _targetNamespace,
                Name = tenantConfig.GetIngressName(),
                Labels = new Dictionary<string, string>()
                {
                    { "autocreated", "true" }, // TODO: Yeet me
                    { "app.kubernetes.io/created-by", "kubesquid" },
                    { "kubesquid-tenantid", tenantConfig.TenantId }
                },
                Annotations = new Dictionary<string, string>()
                {
                    { "kubernetes.io/ingress.class", "nginx" },
                    { "nginx.ingress.kubernetes.io/rewrite-target", "/$1" },
                    { "nginx.ingress.kubernetes.io/use-regex", "true" },
                    {
                        "nginx.ingress.kubernetes.io/configuration-snippet", @$"
                        proxy_set_header TenantId {tenantConfig.TenantId};
                    "
                    },
                    { "nginx.ingress.kubernetes.io/proxy-body-size", "600m" },
                    { "nginx.org/client-max-body-size", "600m" }
                }
            },
            Spec = new V1IngressSpec()
            {
                Rules = new List<V1IngressRule>()
                {
                    new V1IngressRule()
                    {
                        Host = tenantConfig.HostName,
                        Http = new V1HTTPIngressRuleValue()
                        {
                            Paths = new Collection<V1HTTPIngressPath>()
                            {
                                new V1HTTPIngressPath()
                                {
                                    Path = tenantConfig.Path,
                                    PathType = "ImplementationSpecific",
                                    Backend = new V1IngressBackend()
                                    {
                                        Service = new V1IngressServiceBackend()
                                        {
                                            Name = tenantConfig.ServiceName,
                                            Port = new V1ServiceBackendPort() { Number = servicePort }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        try
        {
            await _client.CreateNamespacedIngressAsync(ingress, _targetNamespace);
            _logger.LogInformation("Successfully created ingress: {}", ingress.Metadata.Name);
        }
        catch (HttpOperationException e)
        {
            _logger.LogError("Failed to create ingress {}, due to: {}", ingress.Metadata.Name, e.Message);
        }

    }

    public async Task DeleteIngress(string ingressName)
    {
        try
        {
            await _client.DeleteNamespacedIngressAsync(ingressName, _targetNamespace);
            _logger.LogInformation("Successfully deleted ingress: {}", ingressName);
        }
        catch (HttpOperationException e)
        {
            _logger.LogError("Failed to delete ingress {}, due to: {}", ingressName, e.Message);
        }

    }
}
