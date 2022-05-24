# kubesquid

**NOTE**: Kubesquid version 0.0.1 is still being developed, hence kubesquid should not be used in production, yet.

Run the same application API for multiple tenants smoothly in kubernetes

Kubesquid is a white label solution for external access to services in a Kubernetes cluster via HTTP and HTTPS.

## Requirements

[Helm](https://helm.sh/docs/intro/install/)

## Installation

**TODO**

But, probably something like

```
helm repo add devies https://charts.devies.com
helm install kubesquid devies/kubesquid
```

## How do I use kubesquid?

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
      "hostName": "<required-hostname-to-reach-service>",
      "port": "<port-for-the-service>",
      "path": "<TODO>
    }
```

TODO: Add much more to this section... And examples

## Development

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

