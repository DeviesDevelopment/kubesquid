#!/bin/bash

kind delete cluster
# Create cluster with configuration allowing an Ingress Controller
kind create cluster --config=./tests/cluster-config.yml

(cd tests && ./e2e-test.sh)
