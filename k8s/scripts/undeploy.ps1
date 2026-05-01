# Undeploy banking transaction service from Minikube

$NAMESPACE = "banking-transaction"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Removing Kubernetes resources..." -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

kubectl delete namespace $NAMESPACE

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Removal complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
