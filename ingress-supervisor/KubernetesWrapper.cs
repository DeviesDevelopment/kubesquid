using System.Collections.ObjectModel;
using System.Text.Json;
using ingress_supervisor.Models;
using k8s;
using k8s.Models;

namespace ingress_supervisor;

public class KubernetesWrapper
{
    private const string ConfigMapName = "kubesquid";
    private readonly Kubernetes _client;
    private readonly string _targetNamespace;

    public KubernetesWrapper(Kubernetes client, KubernetesClientConfiguration config)
    {
        _client = client;
        _targetNamespace = config.Namespace;
    }

    public async Task<IList<V1Ingress>> GetIngresses()
    {
        var ingresses = await _client.ListNamespacedIngressAsync(_targetNamespace);
        return ingresses.Items;
    }

    public async Task<List<TenantConfig?>> GetSquidConfig()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var configMap = await _client.ReadNamespacedConfigMapAsync(ConfigMapName, _targetNamespace);
        return configMap.Data
            .Select(pair => JsonSerializer.Deserialize<TenantConfig>(pair.Value, options))
            .ToList();
    }

    public async void CreateIngress(string serviceName, string host, string instanceId)
    {
        var ingress = new V1Ingress()
        {
            Kind = "Ingress",
            Metadata = new V1ObjectMeta()
            {
                NamespaceProperty = _targetNamespace,
                Name = $"{serviceName}-ingress",
                Labels = new Dictionary<string, string>()
            {
                { "autocreated", "true" }
            },
                Annotations = new Dictionary<string, string>()
            {
                {"kubernetes.io/ingress.class", "nginx"},
                {"nginx.ingress.kubernetes.io/rewrite-target", "/$1"},
                {"nginx.ingress.kubernetes.io/use-regex", "true"},
                {
                    $"nginx.ingress.kubernetes.io/configuration-snippet", @$"
                        proxy_set_header InstanceId {instanceId};
                    "
                },
                {"nginx.ingress.kubernetes.io/proxy-body-size", "600m"},
                {"nginx.org/client-max-body-size", "600m"}
            }
            },
            Spec = new V1IngressSpec()
            {
                Rules = new List<V1IngressRule>()
            {
                new V1IngressRule()
                {
                    Host = host,
                    Http = new V1HTTPIngressRuleValue()
                    {
                        Paths = new Collection<V1HTTPIngressPath>()
                        {
                            new V1HTTPIngressPath()
                            {
                                Path = "/(.*)",
                                PathType = "ImplementationSpecific",
                                Backend = new V1IngressBackend()
                                {
                                    Service = new V1IngressServiceBackend()
                                    {
                                        Name = serviceName,
                                        Port = new V1ServiceBackendPort()
                                        {
                                            Number = 80
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            }
        };

        var b = await _client.CreateNamespacedIngressAsync(ingress, _targetNamespace);
    }
}
