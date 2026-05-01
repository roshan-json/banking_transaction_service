#!/bin/bash

# Deploy banking transaction service to Minikube

set -e

NAMESPACE="banking-transaction"
MANIFEST_DIR="./k8s/manifests"

echo "========================================="
echo "Banking Transaction Service - K8s Deploy"
echo "========================================="

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "kubectl is not installed. Please install kubectl first."
    exit 1
fi

# Check if Minikube is running
echo "Checking Minikube status..."
if ! minikube status &> /dev/null; then
    echo "Starting Minikube..."
    minikube start --driver=docker --cpus=4 --memory=8192
fi

echo "Getting Minikube Docker environment..."
eval $(minikube docker-env)

# Build the Docker image
echo "Building Docker image..."
docker build -t banking-transaction-service:latest .

# Apply Kubernetes manifests
echo "Applying Kubernetes manifests..."
kubectl apply -f $MANIFEST_DIR/00-namespace.yaml
kubectl apply -f $MANIFEST_DIR/01-configmap.yaml
kubectl apply -f $MANIFEST_DIR/02-secret.yaml
kubectl apply -f $MANIFEST_DIR/03-pvc.yaml
kubectl apply -f $MANIFEST_DIR/07-serviceaccount.yaml
kubectl apply -f $MANIFEST_DIR/04-deployment.yaml
kubectl apply -f $MANIFEST_DIR/05-service.yaml
kubectl apply -f $MANIFEST_DIR/06-ingress.yaml
kubectl apply -f $MANIFEST_DIR/08-hpa.yaml
kubectl apply -f $MANIFEST_DIR/09-networkpolicy.yaml
kubectl apply -f $MANIFEST_DIR/10-poddisruptionbudget.yaml

echo ""
echo "========================================="
echo "Deployment complete!"
echo "========================================="

# Display deployment status
echo ""
echo "Checking deployment status..."
kubectl get pods -n $NAMESPACE

echo ""
echo "Service Information:"
kubectl get svc -n $NAMESPACE

echo ""
echo "To access the service:"
echo "  - NodePort: http://localhost:30080"
echo "  - Minikube IP: http://$(minikube ip):30080"
echo ""
echo "To check logs:"
echo "  kubectl logs -f deployment/banking-transaction-service -n $NAMESPACE"
echo ""
