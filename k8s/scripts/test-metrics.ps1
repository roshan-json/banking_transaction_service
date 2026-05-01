# Test metrics endpoint

$NAMESPACE = "banking-transaction"
$SERVICE_NAME = "banking-transaction-service"

Write-Host "===========================================" -ForegroundColor Green
Write-Host "Testing Metrics Endpoint" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Green
Write-Host ""

# Check if kubectl is available
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Host "kubectl is not installed." -ForegroundColor Red
    exit 1
}

Write-Host "Starting port-forward to service..." -ForegroundColor Yellow
$pfProcess = Start-Process kubectl -ArgumentList "port-forward", "svc/$SERVICE_NAME", "8080:80", "-n", $NAMESPACE -PassThru

# Wait for port-forward to establish
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "Testing /health endpoint:" -ForegroundColor Cyan
Write-Host ""
curl -v http://localhost:8080/health
Write-Host ""
Write-Host ""

Write-Host "Testing /metrics endpoint:" -ForegroundColor Cyan
Write-Host ""
curl -v http://localhost:8080/metrics | Select-Object -First 50
Write-Host ""
Write-Host "..." -ForegroundColor Yellow
Write-Host ""

# Kill port-forward
Stop-Process -Id $pfProcess.Id -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "===========================================" -ForegroundColor Green
Write-Host "Metrics test complete" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Green
