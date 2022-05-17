using DefaultNamespace;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<IHostedService>(x => new ServiceWatcher(client, config.Namespace));
    services.AddSingleton<IHostedService>(x => new ConfigmapWatcher(client, config.Namespace));
});

await builder.RunConsoleAsync();

Console.WriteLine("Bye, World!");
