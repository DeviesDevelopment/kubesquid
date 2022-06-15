# kubesquid

**NOTE**: Kubesquid version 0.1.0 is still being developed, hence kubesquid should not be used in production, yet.

Kubesquid is a white label solution for external access to services in a Kubernetes cluster via HTTP and HTTPS. Kubesquid manages ingresses based on your configuration and what services are currently deployed to your cluster.

![image](https://user-images.githubusercontent.com/8545435/170988870-119a1ff6-a452-4257-8433-8316f418c82a.png)

## Installation
```
helm repo add deviesdevelopment https://deviesdevelopment.github.io/kubesquid
helm install kubesquid deviesdevelopment/kubesquid-ingress-supervisor
````

## Get started

Kubesquid handles [https://kubernetes.io/docs/concepts/services-networking/ingress/](ingresses) in the cluster, based on a configuration. When kubequid gets installed to the cluster, a few kubernetes resources will be created, but most important a configMap.

To configure the external access to your services, all you need to do is to update the `kubesquid` configmap. The configmap expects `key-value` data on the following format

```yaml
data:
  <some-key>: |
    {
      "serviceName": "<name-of-a-service-in-the-cluster>",
      "tenantId": "<id-to-pass-down-to-service>",
      "hostName": "<ingress-rule-host>",
      "path": "<ingress-rule-path>"
    }
```

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

### Handle releases

A new release will be published on commits to `master` if `/charts/kubesquid-ingress-supervisor/Chart.yml` has been updated, i.e the `version` field.

GitHub Pages is being used for hosting the Helm Chart. On every release, we update the `index.yaml` file with the new release on the `gh-pages` branch. Every release contains the packaged helm chart as `kubesquid-ingress-supervisor-<version>.tgz` which the `index.yml` file refers to.
