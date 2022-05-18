#!/bin/bash
# Terminate if any command fails
set -ex

kind delete cluster
# Create cluster with configuration allowing an Ingress Controller
kind create cluster --config=./cluster-config.yml
# Setup NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
sleep 5 # https://github.com/kubernetes/kubernetes/issues/83242
# Wait until the nginx controller is ready
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=90s

# Verify that cluster is running
kubectl version
# TODO: Move up!
docker build --no-cache -t miledevies/kubesquid-ingress-supervisor:e2e-test ../ingress-supervisor
kind load docker-image miledevies/kubesquid-ingress-supervisor:e2e-test --name kind
helm package ../ingress-supervisor/kubesquid-ingress-supervisor
helm install --set image.tag=e2e-test kubesquid-ingress-supervisor kubesquid-ingress-supervisor-0.1.0.tgz
kubectl apply -f test-configmap.yml
# Install whoami service and annotate it with "squid"
helm install -f whoami-values.yml whoami cowboysysop/whoami
kubectl wait --namespace default \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/name=whoami \
  --timeout=90s

# TODO! Vi måste vänta på våra kära ingresses!
kubectl wait --namespace default \
  --for=condition=ready ingress \
  --selector=app.kubernetes.io/name=whoami \
  --timeout=90s

curl localhost/customer-a -s -H -v "host: baloo.devies.com"
# Verify
curl localhost/customer-a -s -H "host: baloo.devies.com" | grep "Instanceid: 666"
echo "GET baloo.devies.com/customer-a successfully included request header Instanceid 666"

curl localhost/customer-b -s -H "host: baloo.devies.com" | grep "Instanceid: 888"
echo "GET baloo.devies.com/customer-b successfully included request header Instanceid 888"

curl localhost/customer-c -s -H "host: baloo.devies.com" | grep "404 Not Found"
echo "GET baloo.devies.com/customer-b successfully returned 404 Not Found"
