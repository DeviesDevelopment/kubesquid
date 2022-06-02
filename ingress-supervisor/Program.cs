using DefaultNamespace;
using ingress_supervisor;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = false;
        options.TimestampFormat = "hh:mm:ss ";
        options.SingleLine = true;
    });
});

await builder.RunConsoleAsync();
