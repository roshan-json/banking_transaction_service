#!/bin/bash

# Banking Transaction Service - Docker Setup Script for Linux/Mac
# Usage: chmod +x docker-setup.sh && ./docker-setup.sh

set -e

echo ""
echo "============================================"
echo "Banking Transaction Service - Docker Setup"
echo "============================================"
echo ""

# Step 1: Check if Docker is running
echo "[Step 1/6] Checking Docker installation..."
if ! command -v docker &> /dev/null; then
    echo "[✗] Docker is not installed. Please install Docker first."
    exit 1
fi

docker version > /dev/null 2>&1 || {
    echo "[✗] Docker is not running. Please start Docker daemon."
    exit 1
}
echo "[✓] Docker is installed and running"

# Step 2: Build and start containers
echo ""
echo "[Step 2/6] Building Docker image and starting containers..."
docker-compose up -d --build

if [ $? -ne 0 ]; then
    echo "[✗] Failed to start containers"
    exit 1
fi

echo "[✓] Containers started successfully"

# Step 3: Wait for service to be ready
echo ""
echo "[Step 3/6] Waiting for service to be healthy..."
sleep 5

# Check container status
CONTAINER_STATUS=$(docker ps --filter "name=banking-transaction-service" --format "{{.Status}}")
if [[ $CONTAINER_STATUS == *"healthy"* ]] || [[ $CONTAINER_STATUS == *"running"* ]]; then
    echo "[✓] Container is running"
else
    echo "[!] Container status: $CONTAINER_STATUS"
fi

# Step 4: Display running containers
echo ""
echo "[Step 4/6] Running containers:"
echo ""
docker ps --filter "name=banking-transaction" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# Step 5: Test health endpoint
echo "[Step 5/6] Testing health endpoint..."
if command -v curl &> /dev/null; then
    if RESPONSE=$(curl -s http://localhost:8080/health); then
        echo "[✓] Health check passed"
        echo "Response: $RESPONSE"
    else
        echo "[!] Health check not yet available. Service may still be starting..."
    fi
else
    echo "[!] curl not available. Skipping health check test."
fi

# Step 6: Display service information
echo ""
echo "[Step 6/6] Service Information"
echo ""
echo "============================================"
echo "Service Details:"
echo "============================================"
echo "Service Name:    banking-transaction-service"
echo "Container Port:  8080"
echo "Host Port:       8080"
echo "Status:          Running"
echo ""

echo "============================================"
echo "Access URLs:"
echo "============================================"
echo "Swagger UI:    http://localhost:8080/swagger/index.html"
echo "Health Check:  http://localhost:8080/health"
echo "Transactions:  http://localhost:8080/transactions"
echo ""

echo "============================================"
echo "Useful Commands:"
echo "============================================"
echo "View logs:           docker-compose logs -f"
echo "Stop service:        docker-compose down"
echo "Stop and remove:     docker-compose down -v"
echo "Restart service:     docker-compose restart"
echo "View container logs: docker logs banking-transaction-service"
echo ""

echo "============================================"
echo "Sample API Requests:"
echo "============================================"
echo ""
echo "1. Get Health Status:"
echo "   curl http://localhost:8080/health"
echo ""
echo "2. Get All Transactions:"
echo "   curl 'http://localhost:8080/transactions?accountId=1'"
echo ""
echo "3. Create Transaction:"
echo "   curl -X POST http://localhost:8080/transactions \\"
echo "     -H 'Content-Type: application/json' \\"
echo "     -d '{'"
echo "       \"accountId\": 1,"
echo "       \"amount\": 100.50,"
echo "       \"transactionType\": \"DEBIT\","
echo "       \"description\": \"Payment\","
echo "       \"idempotencyKey\": \"unique-key-123\""
echo "     }'"
echo ""

echo "============================================"
echo "Next Steps:"
echo "============================================"
echo "1. Open Swagger UI in browser: http://localhost:8080/swagger/index.html"
echo "2. Test API endpoints through Swagger or curl"
echo "3. Monitor logs: docker-compose logs -f"
echo "4. When done, stop service: docker-compose down"
echo ""
echo "Setup complete!"
