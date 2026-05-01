@echo off
REM Banking Transaction Service - Docker Setup Script for Windows

echo.
echo ============================================
echo Banking Transaction Service - Docker Setup
echo ============================================
echo.

REM Step 1: Build and start containers
echo [Step 1/5] Building Docker image and starting containers...
docker-compose up -d --build

if errorlevel 1 (
    echo ERROR: Failed to start containers. Make sure Docker is installed and running.
    exit /b 1
)

echo [✓] Containers started successfully

REM Wait for service to be healthy
echo.
echo [Step 2/5] Waiting for service to be healthy (30 seconds)...
timeout /t 3 /nobreak

REM Step 2: Check running containers
echo.
echo [Step 3/5] Checking running containers...
echo.
docker ps --filter "name=banking-transaction" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.

REM Step 3: Test health endpoint
echo [Step 4/5] Testing health endpoint...
for /f %%A in ('curl -s http://localhost:8080/health') do (
    echo Health Check Response: %%A
)
echo.

REM Step 4: Display information
echo [Step 5/5] Service Information
echo.
echo ============================================
echo Service Details:
echo ============================================
echo Service Name:    banking-transaction-service
echo Container Port:  8080
echo Host Port:       8080
echo Status:          Running (check output above)
echo.
echo ============================================
echo Access URLs:
echo ============================================
echo Swagger UI:    http://localhost:8080/swagger/index.html
echo Health Check:  http://localhost:8080/health
echo Transactions:  http://localhost:8080/transactions
echo.
echo ============================================
echo Useful Commands:
echo ============================================
echo View logs:           docker-compose logs -f
echo Stop service:        docker-compose down
echo Stop and remove:     docker-compose down -v
echo Restart service:     docker-compose restart
echo View container logs: docker logs banking-transaction-service
echo.
echo ============================================
echo Testing API Endpoints:
echo ============================================
echo.

REM Test health endpoint with details
echo Testing Health Endpoint:
curl -i http://localhost:8080/health
echo.
echo.

REM Show next steps
echo ============================================
echo Next Steps:
echo ============================================
echo 1. Open Swagger UI: http://localhost:8080/swagger/index.html
echo 2. Test API endpoints through Swagger UI or curl
echo 3. Monitor logs: docker-compose logs -f
echo 4. When done, stop service: docker-compose down
echo.

pause
