using k8s;

Console.WriteLine("Hello, World!");

var config = KubernetesClientConfiguration.InClusterConfig();
Console.WriteLine(config.CurrentContext);
Console.WriteLine(config.Host);
Console.WriteLine(config.AccessToken);
Console.WriteLine(config.Username);
Console.WriteLine(config.Namespace);

var files = Directory.GetFiles("/var/run/secrets/kubernetes.io/serviceaccount");
Console.WriteLine(string.Join(",", files));
string token = System.IO.File.ReadAllText(@"/var/run/secrets/kubernetes.io/serviceaccount/token");
string ns = System.IO.File.ReadAllText(@"/var/run/secrets/kubernetes.io/serviceaccount/namespace");
Console.WriteLine($"Token: {token}");
Console.WriteLine($"Namespaces: {ns}");

var client = new Kubernetes(config);

var namespaces = client.CoreV1.ListNamespace();
Console.WriteLine(namespaces);
