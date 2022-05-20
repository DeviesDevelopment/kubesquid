#!/bin/bash

# Terminate if any command fails
set -ex

docker build --no-cache -t miledevies/kubesquid-ingress-supervisor:e2e-test ingress-supervisor

kind delete cluster
# Create cluster with configuration allowing an Ingress Controller
kind create cluster --config=./tests/cluster-config.yml

(cd tests && ./e2e-test.sh e2e-test)
