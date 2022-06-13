#!/bin/bash
# Terminate if any command fails
set -ex

IMAGE_TAG=$1

# Setup NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml

RETRY_COUNT=0
until [ $RETRY_COUNT -eq 15 ] || kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=90s; do
    sleep 1
    ((RETRY_COUNT=RETRY_COUNT+1))
done
[ "$RETRY_COUNT" -lt 15 ]

# Verify that cluster is running
kubectl version
kind load docker-image deviesdevelopment/kubesquid-ingress-supervisor:"$IMAGE_TAG" --name kind
helm package ../charts/kubesquid-ingress-supervisor
helm install --set image.tag="$IMAGE_TAG" kubesquid-ingress-supervisor kubesquid-ingress-supervisor-0.1.0.tgz
kubectl apply -f test-configmap.yml
# Install whoami service and annotate it with "squid"
helm repo add cowboysysop https://cowboysysop.github.io/charts/
helm install -f whoami-values.yml whoami cowboysysop/whoami
kubectl wait --namespace default \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/name=whoami \
  --timeout=90s

sleep 5

# Verify create (triggered on new service)
curl localhost/customer-a -s -H "host: baloo.devies.com" | grep "Tenantid: 666"
curl localhost/customer-b -s -H "host: baloo.devies.com" | grep "Tenantid: 888"
curl localhost/customer-c -s -H "host: baloo.devies.com" | grep "404 Not Found"
curl localhost/customer-a -s -H "host: baloo.devies.com" -H "Tenantid: 1337" | grep --invert-match "Tenantid: 1337"

# Extend configuration with new tenant
kubectl apply -f test-configmap-extended.yml
sleep 5

# Verify create (triggered on config change)
curl localhost/customer-c -s -H "host: mowgli.devies.com" | grep "Tenantid: 999"

# Update config to remove new service
kubectl apply -f test-configmap.yml
sleep 5

# Verify delete (triggered on config change)
curl localhost/customer-c -s -H "host: mowgli.devies.com" | grep "404 Not Found"

# Update existing configuration with new tenantId for kunda and new hostname for kundb.
kubectl apply -f test-configmap-modified.yml
sleep 5

# Verify updates (triggered on config change)
curl localhost/customer-d -s -H "host: baloo.devies.com" | grep "Tenantid: 666"
curl localhost/customer-b -s -H "host: bagheera.devies.com" | grep "Tenantid: 888"

# Update whoami service with new port
helm upgrade --install -f whoami-values-modified.yml whoami cowboysysop/whoami
kubectl wait --for=jsonpath='{spec.ports[0].port}=99' --timeout=30s --namespace default services/whoami
sleep 5

# Verify update (trigger on service change)
curl localhost/customer-d -s -H "host: baloo.devies.com" | grep "Tenantid: 666"
curl localhost/customer-b -s -H "host: bagheera.devies.com" | grep "Tenantid: 888"

# Uninstall whoami
helm uninstall whoami
kubectl wait --for=delete pod --selector=app.kubernetes.io/name=whoami --timeout=60s
sleep 5

# Verify delete (triggered on service delete)
curl localhost/customer-d -s -H "host: baloo.devies.com" | grep "404 Not Found"
echo "GET baloo.devies.com/customer-d successfully returned 404 Not Found"

curl localhost/customer-b -s -H "host: bagheera.devies.com" | grep "404 Not Found"
echo "GET baloo.devies.com/customer-b successfully returned 404 Not Found"

echo "end-to-end ran successfully!"
