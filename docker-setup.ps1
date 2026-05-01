# Banking Transaction Service - Docker Setup Script for PowerShell
# Usage: .\docker-setup.ps1

Write-Host "`n" -NoNewline
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Banking Transaction Service - Docker Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if Docker is running
Write-Host "[Step 1/6] Checking Docker installation..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "[✓] Docker is installed and running" -ForegroundColor Green
} catch {
    Write-Host "[✗] Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Step 2: Build and start containers
Write-Host "`n[Step 2/6] Building Docker image and starting containers..." -ForegroundColor Yellow
docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "[✗] Failed to start containers" -ForegroundColor Red
    exit 1
}

Write-Host "[✓] Containers started successfully" -ForegroundColor Green

# Step 3: Wait for service to be ready
Write-Host "`n[Step 3/6] Waiting for service to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check container status
$containerStatus = docker ps --filter "name=banking-transaction-service" --format "{{.Status}}"
if ($containerStatus -match "healthy|running") {
    Write-Host "[✓] Container is running" -ForegroundColor Green
} else {
    Write-Host "[!] Container status: $containerStatus" -ForegroundColor Yellow
}

# Step 4: Display running containers
Write-Host "`n[Step 4/6] Running containers:" -ForegroundColor Yellow
Write-Host ""
docker ps --filter "name=banking-transaction" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
Write-Host ""

# Step 5: Test health endpoint
Write-Host "[Step 5/6] Testing health endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing
    Write-Host "[✓] Health check passed" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "[!] Health check not yet available. Service may still be starting..." -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 6: Display service information
Write-Host "`n[Step 6/6] Service Information" -ForegroundColor Yellow
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Service Details:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Service Name:    banking-transaction-service"
Write-Host "Container Port:  8080"
Write-Host "Host Port:       8080"
Write-Host "Status:          Running"
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Access URLs:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Swagger UI:    http://localhost:8080/swagger/index.html"
Write-Host "Health Check:  http://localhost:8080/health"
Write-Host "Transactions:  http://localhost:8080/transactions"
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Useful Commands:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "View logs:           docker-compose logs -f"
Write-Host "Stop service:        docker-compose down"
Write-Host "Stop and remove:     docker-compose down -v"
Write-Host "Restart service:     docker-compose restart"
Write-Host "View container logs: docker logs banking-transaction-service"
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Sample API Requests:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Get Health Status:"
Write-Host "   curl http://localhost:8080/health" -ForegroundColor Green
Write-Host ""
Write-Host "2. Get All Transactions:"
Write-Host "   curl 'http://localhost:8080/transactions?accountId=1'" -ForegroundColor Green
Write-Host ""
Write-Host "3. Create Transaction:"
Write-Host "   curl -X POST http://localhost:8080/transactions \"
Write-Host "     -H 'Content-Type: application/json' \"
Write-Host "     -d '{" -ForegroundColor Green
Write-Host "       \"accountId\": 1," -ForegroundColor Green
Write-Host "       \"amount\": 100.50," -ForegroundColor Green
Write-Host "       \"transactionType\": \"DEBIT\"," -ForegroundColor Green
Write-Host "       \"description\": \"Payment\"," -ForegroundColor Green
Write-Host "       \"idempotencyKey\": \"unique-key-123\"" -ForegroundColor Green
Write-Host "     }'" -ForegroundColor Green
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "1. Open Swagger UI in browser:"
Write-Host "   Start-Process 'http://localhost:8080/swagger/index.html'" -ForegroundColor Green
Write-Host "2. Test API endpoints through Swagger or curl"
Write-Host "3. Monitor logs: docker-compose logs -f"
Write-Host "4. When done, stop service: docker-compose down"
Write-Host ""

# Optional: Open Swagger in browser
$openSwagger = Read-Host "Would you like to open Swagger UI in your browser? (Y/N)"
if ($openSwagger -eq "Y" -or $openSwagger -eq "y") {
    Start-Process "http://localhost:8080/swagger/index.html"
    Write-Host "[✓] Swagger UI opened in default browser" -ForegroundColor Green
}

Write-Host ""
Write-Host "Setup complete! Press Enter to exit..."
Read-Host
