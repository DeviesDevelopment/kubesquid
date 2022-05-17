using DefaultNamespace;
using ingress_supervisor;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<Kubernetes>(new Kubernetes(config));
    services.AddSingleton<KubernetesClientConfiguration>(config);
    services.AddSingleton<KubernetesWrapper>();
    services.AddSingleton<IHostedService, ServiceWatcher>();
    services.AddSingleton<IHostedService, ConfigmapWatcher>();
});

await builder.RunConsoleAsync();

Console.WriteLine("Bye, World!");
