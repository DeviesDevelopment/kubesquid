using DefaultNamespace;
using ingress_supervisor;
using k8s;
using Microsoft.Extensions.Logging;
using Moq;

namespace unit_tests;

public class WatcherTests
{
    private readonly ServiceWatcher _serviceWatcher;
    private readonly Mock<KubernetesClientConfiguration> _kubernetesClientConfigurationMock;
    private readonly Mock<Kubernetes> _kubernetesClientMock;

    public WatcherTests()
    {
        _kubernetesClientConfigurationMock= new Mock<KubernetesClientConfiguration>();
        _kubernetesClientMock = new Mock<Kubernetes>(KubernetesClientConfiguration.BuildDefaultConfig());
        var kubernetesWrapperMock = new Mock<KubernetesWrapper>(_kubernetesClientMock.Object, _kubernetesClientConfigurationMock.Object, new Mock<ILogger<KubernetesWrapper>>().Object);
        var loggerMock = new Mock<ILogger<ServiceWatcher>>();
        _serviceWatcher = new ServiceWatcher(
            _kubernetesClientMock.Object,
            _kubernetesClientConfigurationMock.Object,
            kubernetesWrapperMock.Object,
            loggerMock.Object);
    }


    [Fact]
    public void WatchserviceAddedEvent_IngressCreate()
    {
        //_kubernetesClientMock.Setup(client => client.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(It.IsAny<string>()).
        var cancellationToken = new CancellationToken();
        _serviceWatcher.StartAsync(cancellationToken);

    }
}
