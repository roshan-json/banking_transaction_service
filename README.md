# Banking Transaction Service

The **Banking Transaction Service** is an ASP.NET Core Web API for managing banking transactions. It provides REST endpoints to retrieve transactions, create transactions with idempotency guarantees, and partially update existing transactions.

The service persists transaction data using **Back4App (Parse REST API)** and is fully observable with **Prometheus metrics**, **custom HTTP metrics middleware**, **Serilog logging**, and **Swagger/OpenAPI** documentation.

---

## Overview

The Banking Transaction Service exposes APIs under the `/transactions` route and acts as an observable application layer over Back4App for transaction storage and retrieval.

Core capabilities include:
- Retrieve transactions by ID or account
- Idempotent transaction creation
- Partial transaction updates
- External persistence via Back4App
- HTTP RED metrics via custom middleware
- Business and dependency metrics via Prometheus
- Structured logging using Serilog
- Swagger/OpenAPI documentation
- Health endpoint for container orchestration

---

## Architecture
Client
↓
MetricsMiddleware (HTTP RED metrics)
↓
TransactionController (HTTP / API)
↓
Back4AppService (Business + Persistence)
↓
Back4App Parse REST API


### Cross-cutting Components

- **MetricsMiddleware** – HTTP request metrics (RED metrics)
- **MetricsService** – Business and dependency metrics
- **Serilog** – Structured logging
- **Swagger** – API documentation

---

## External Dependencies

- ASP.NET Core Web API
- HttpClient
- Back4App / Parse REST API
- Prometheus (`prometheus-net`)
- Serilog
- Swashbuckle / Swagger
- Microsoft.Extensions.Logging & Configuration
- System.Text.Json

---

## Application Startup

### Service Registration

The following services are registered during application startup:

- Controllers
- HttpClient
- `Back4AppService` (Scoped)
- `MetricsService` (Singleton)
- Swagger / OpenAPI services

---

### Logging

The application uses **Serilog** with the following configuration:

- Console logging
- Rolling daily log files (`logs/log-YYYYMMDD.txt`)
- Log enrichment:
  - Thread ID
  - Machine name
  - Log context

Minimum log level: **Information**

---

## Middleware Pipeline

The HTTP request pipeline is configured in the following order:

1. **MetricsMiddleware**
2. **Prometheus Metrics Server** (`/metrics`)
3. Swagger & Swagger UI (Development only)
4. HTTPS redirection
5. Authorization
6. Controller endpoints

---

## MetricsMiddleware (HTTP RED Metrics)

`MetricsMiddleware` records request-level RED (Request, Error, Duration) metrics for all HTTP traffic.

### Recorded Data

For each request, the middleware captures:
- HTTP method
- Request path
- Response status code
- Total request duration

Metrics are recorded after the request completes.

### Error Classification

HTTP errors (status code ≥ 400) are categorized using normalized labels:

| Status Code | Label |
|-------------|-------|
| 400 | `bad_request` |
| 404 | `not_found` |
| 500 | `internal_error` |
| 502 | `bad_gateway` |
| 503 | `service_unavailable` |
| Other | `error_{statusCode}` |

---

## API Endpoints

Base route: `/transactions`

### Retrieve a Transaction by ID
GET /transactions/{transactionId}

- Retrieves a single transaction by internal transaction ID
- Returns `404 Not Found` if the transaction does not exist

---

### Retrieve Transactions by Account
GET /transactions?accountId={accountId}

- Retrieves all transactions for a specific account
- Records balance-check latency metrics
- Returns `404 Not Found` if no transactions are found

---

### Create a Transaction
POST /transactions

Creates a new transaction with **idempotency support**.

#### Validation Rules

- Request body must not be null
- `IdempotencyKey` must be present and non-empty

#### Behavior

- If a transaction with the same idempotency key already exists, it is returned
- Otherwise, a new transaction is created and persisted

---

### Partially Update a Transaction
PATCH /transactions/{transactionId}

- Updates one or more fields of an existing transaction
- Only fields provided in the request payload are applied
- Returns `404 Not Found` if the transaction does not exist

---

## Request Models

### CreateTransactionRequest

Fields:
- `Type` (`string`)
- `AccountId` (`int`)
- `Amount` (`decimal`)
- `CounterParty` (`string`)
- `Reference` (`string`)
- `IdempotencyKey` (`string`, required)

---

### UpdateTransactionRequest

All fields are optional:
- `AccountId` (`int?`)
- `Amount` (`decimal?`)
- `Type` (`string?`)
- `CounterParty` (`string?`)
- `Reference` (`string?`)

---

## Response Model

### TransactionResponse

Returned for retrieval, creation, and update operations.

Fields:
- `Id` (`int`)
- `Type` (`string`)
- `AccountId` (`int`)
- `Amount` (`decimal`)
- `CounterParty` (`string`)
- `Reference` (`string`)
- `CreatedAt` (`DateTime`)

Internal only (not serialized):
- `IdempotencyKey`

---

## Persistence Details

- Transactions are stored in Back4App under the `Transaction` class
- Queries use Parse-style `where` filters
- Each transaction has:
  - Logical transaction ID (`txnId`)
  - Back4App `objectId`

### Transaction ID Generation

- The next transaction ID is calculated by:
  1. Fetching the transaction with the highest existing `txnId`
  2. Incrementing it by one
- ID generation is handled entirely in the service layer

---

## Idempotency

- Enforced during transaction creation using `IdempotencyKey`
- Prevents duplicate inserts when requests are retried

Outcomes recorded:
- `success`
- `duplicate`
- `failed`

---

## Metrics & Observability

### Prometheus Scrape Endpoint
GET /metrics

---

### HTTP RED Metrics

- `http_requests_total`  
  Labels: `method`, `path`, `status_code`

- `http_request_duration_seconds`  
  Labels: `method`, `path`, `status_code`

- `http_errors_total`  
  Labels: `method`, `path`, `error_type`

---

### Business Metrics

- `transactions_total`  
  Labels: `transaction_type`, `status`

- `failed_transfers_total`  
  Labels: `reason`

- `active_transactions` (Gauge)  
  Labels: `transaction_type`

- `balance_check_latency_ms`  
  Labels: `account_id`

---

### Dependency Metrics (Back4App)

- `back4app_api_latency_ms`  
  Labels: `operation` (`get`, `post`, `put`)

- `back4app_errors_total`  
  Labels: `operation`, `error_code`

---

## Swagger / OpenAPI

Swagger is enabled **only in Development environments**.

- Title: *Banking Transaction Service*
- Version: `v1`
- XML comments included when available
- Schema examples provided via `SwaggerExamplesSchemaFilter`

---

## Health Check

A lightweight health endpoint is exposed for container orchestration systems.

GET /health

Example response:

```json
{
  "status": "healthy",
  "timestamp": "2026-05-02T00:00:00Z"
}
```

## Configuration

The following configuration values are required at startup:

Back4App:BaseUrl
Back4App:AppId
Back4App:RestApiKey

The application fails fast if any required value is missing.

## Project Structure

Controllers/
  └── TransactionController.cs

Services/
  ├── Back4AppService.cs
  └── MetricsService.cs

Middleware/
  └── MetricsMiddleware.cs

Swagger/
  └── SwaggerExamplesSchemaFilter.cs

Models/
  ├── Requests/
  │   ├── CreateTransactionRequest.cs
  │   └── UpdateTransactionRequest.cs
  └── TransactionResponse.cs

Program.cs