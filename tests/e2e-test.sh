#!/bin/bash
# 1. Spin up minikube / kind
# 2. Validate that ingress controller exists
# 3. Package helm
# 4. Install helm package
# 5. Add configmap
# 6. Deploy service (whoami)
# 7. Verify ingress created with correct parameters.

# Terminate if any command fails
set -e

kind delete cluster
# Create cluster with configuration allowing an Ingress Controller
kind create cluster --config=./cluster-config.yml
#kubectl wait --namespace ingress-nginx \
#  --for=condition=ready pod \
#  --selector=app.kubernetes.io/component=controller \
#  --timeout=90s

kubectl version
docker build --no-cache -t miledevies/kubesquid-ingress-supervisor:e2e-test ../ingress-supervisor
kind load docker-image miledevies/kubesquid-ingress-supervisor:e2e-test --name kind
helm package ../ingress-supervisor/kubesquid-ingress-supervisor
helm install kubesquid-ingress-supervisor kubesquid-ingress-supervisor-0.1.0.tgz
kubectl apply -f test-configmap.yml
