using DefaultNamespace;
using ingress_supervisor;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Starting kubesquid");

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var config = environment == "Development" ? KubernetesClientConfiguration.BuildConfigFromConfigFile() : KubernetesClientConfiguration.InClusterConfig();

var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<Kubernetes>(new Kubernetes(config));
    services.AddSingleton<KubernetesClientConfiguration>(config);
    services.AddSingleton<KubernetesWrapper>();
    services.AddHostedService<ServiceWatcher>();
    services.AddHostedService<ConfigmapWatcher>();
});

await builder.RunConsoleAsync();

Console.WriteLine("Exiting kubeqsuid");
