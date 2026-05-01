#!/bin/bash

# Undeploy banking transaction service from Minikube

set -e

NAMESPACE="banking-transaction"

echo "========================================="
echo "Removing Kubernetes resources..."
echo "========================================="

kubectl delete namespace $NAMESPACE

echo ""
echo "========================================="
echo "Removal complete!"
echo "========================================="
