#!/bin/bash

# Test metrics endpoint

NAMESPACE="banking-transaction"
SERVICE_NAME="banking-transaction-service"

echo "==========================================="
echo "Testing Metrics Endpoint"
echo "==========================================="
echo ""

# Check if port-forward is needed
echo "Starting port-forward to service..."
kubectl port-forward svc/$SERVICE_NAME 8080:80 -n $NAMESPACE &
PF_PID=$!

# Wait for port-forward to establish
sleep 2

echo ""
echo "Testing /health endpoint:"
echo ""
curl -v http://localhost:8080/health
echo ""
echo ""

echo "Testing /metrics endpoint:"
echo ""
curl -v http://localhost:8080/metrics | head -50
echo ""
echo "..."
echo ""

# Kill port-forward
kill $PF_PID 2>/dev/null || true

echo ""
echo "==========================================="
echo "Metrics test complete"
echo "==========================================="
