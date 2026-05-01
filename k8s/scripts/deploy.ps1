# Deploy banking transaction service to Minikube

$NAMESPACE = "banking-transaction"
$MANIFEST_DIR = "./k8s/manifests"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Banking Transaction Service - K8s Deploy" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Check if kubectl is available
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Host "kubectl is not installed. Please install kubectl first." -ForegroundColor Red
    exit 1
}

# Check if Minikube is running
Write-Host "Checking Minikube status..." -ForegroundColor Yellow
$minikubeStatus = minikube status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Starting Minikube..." -ForegroundColor Yellow
    minikube start --driver=docker --cpus=4 --memory=8192
}

Write-Host "Getting Minikube Docker environment..." -ForegroundColor Yellow
minikube docker-env | Invoke-Expression

# Build the Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
docker build -t banking-transaction-service:latest .

# Apply Kubernetes manifests
Write-Host "Applying Kubernetes manifests..." -ForegroundColor Yellow
kubectl apply -f "$MANIFEST_DIR/00-namespace.yaml"
kubectl apply -f "$MANIFEST_DIR/01-configmap.yaml"
kubectl apply -f "$MANIFEST_DIR/02-secret.yaml"
kubectl apply -f "$MANIFEST_DIR/03-pvc.yaml"
kubectl apply -f "$MANIFEST_DIR/07-serviceaccount.yaml"
kubectl apply -f "$MANIFEST_DIR/04-deployment.yaml"
kubectl apply -f "$MANIFEST_DIR/05-service.yaml"
kubectl apply -f "$MANIFEST_DIR/06-ingress.yaml"
kubectl apply -f "$MANIFEST_DIR/08-hpa.yaml"
kubectl apply -f "$MANIFEST_DIR/09-networkpolicy.yaml"
kubectl apply -f "$MANIFEST_DIR/10-poddisruptionbudget.yaml"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Display deployment status
Write-Host ""
Write-Host "Checking deployment status..." -ForegroundColor Yellow
kubectl get pods -n $NAMESPACE

Write-Host ""
Write-Host "Service Information:" -ForegroundColor Yellow
kubectl get svc -n $NAMESPACE

Write-Host ""
Write-Host "To access the service:" -ForegroundColor Cyan
Write-Host "  - NodePort: http://localhost:30080" -ForegroundColor Cyan
Write-Host "  - Minikube IP: http://$(minikube ip):30080" -ForegroundColor Cyan
Write-Host ""
Write-Host "To check logs:" -ForegroundColor Cyan
Write-Host "  kubectl logs -f deployment/banking-transaction-service -n $NAMESPACE" -ForegroundColor Cyan
Write-Host ""
