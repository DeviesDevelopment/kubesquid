using k8s;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var namespaces = client.CoreV1.ListNamespace();
Console.WriteLine(namespaces);
