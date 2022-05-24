# kubesquid

**NOTE**: Kubesquid version 0.0.1 is still being developed, hence kubesquid should not be used in production, yet.

Kubesquid is a white label solution for external access to services in a Kubernetes cluster via HTTP and HTTPS. Kubesquid manages ingresses based on your configuration and what services are currently deployed to your cluster.

## Requirements

- [Helm](https://helm.sh/docs/intro/install/)
- Running Kubernetes Cluster

## Installation

From source:
```
helm package ingress-supervisor/kubesquid-ingress-supervisor
helm install kubesquid-ingress-supervisor kubesquid-ingress-supervisor-0.1.0.tgz
````

## Get started

Kubesquid handles [https://kubernetes.io/docs/concepts/services-networking/ingress/](ingresses) in the cluster, based on a configuration. When kubequid gets installed to the cluster, a few kubernetes resources will be created, but most important the following two.

1. kubesquid (pod) which is the kubesquid application
2. kubesquid (configMap) which holds the configuration.

To configure the external access to your services, all you need to do is to update the `kubesquid` configmap. The configmap expects `key-value` data on the following format

```yaml
data:
  <some-key>: |
    {
      "serviceName": "<name-of-a-service-in-the-cluster>",
      "instanceId": "<id-to-pass-down-to-service>",
      "hostName": "<ingress-rule-host>",
      "port": "<port-for-the-service>",
      "path": "<ingress-rule-path>"
    }
```

TODO: Add much more to this section...

## Development Setup

### Requirements

- Docker
- kind
- kubectl
- helm

### Running e2e tests

From repository root, run:

```
./run-e2e-test-locally.sh
```

