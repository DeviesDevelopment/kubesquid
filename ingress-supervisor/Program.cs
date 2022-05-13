using k8s;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
var client = new Kubernetes(config);

var pods = client.CoreV1.ListNamespacedPod("default");
Console.WriteLine(string.Join(",", pods));

