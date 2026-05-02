using Prometheus;

namespace banking_transaction_service.Services
{
    /// <summary>
    /// Prometheus metrics service for tracking RED (Request Rate, Error Rate, Duration) and business metrics
    /// </summary>
    public class MetricsService
    {
        /// <summary>
        /// HTTP request counter by method, path, and status code
        /// </summary>
        private static readonly Counter HttpRequestsTotal = Metrics
            .CreateCounter("http_requests_total", "Total number of HTTP requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "method", "path", "status_code" }
                });

        /// <summary>
        /// HTTP request duration histogram (in seconds)
        /// </summary>
        private static readonly Histogram HttpRequestDuration = Metrics
            .CreateHistogram("http_request_duration_seconds", "HTTP request duration in seconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "method", "path", "status_code" },
                    Buckets = new[] { 0.01, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0 }
                });

        /// <summary>
        /// HTTP errors counter by method, path, and error type
        /// </summary>
        private static readonly Counter HttpErrors = Metrics
            .CreateCounter("http_errors_total", "Total number of HTTP errors",
                new CounterConfiguration
                {
                    LabelNames = new[] { "method", "path", "error_type" }
                });

        // Business Metrics

        /// <summary>
        /// Total number of transactions created
        /// </summary>
        private static readonly Counter TransactionsTotal = Metrics
            .CreateCounter("transactions_total", "Total number of transactions created",
                new CounterConfiguration
                {
                    LabelNames = new[] { "transaction_type", "status" }
                });

        /// <summary>
        /// Total number of failed transfers
        /// </summary>
        private static readonly Counter FailedTransfersTotal = Metrics
            .CreateCounter("failed_transfers_total", "Total number of failed transfers",
                new CounterConfiguration
                {
                    LabelNames = new[] { "reason" }
                });

        /// <summary>
        /// Balance check latency in milliseconds
        /// </summary>
        private static readonly Histogram BalanceCheckLatencyMs = Metrics
            .CreateHistogram("balance_check_latency_ms", "Balance check latency in milliseconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "account_id" },
                    Buckets = new double[] { 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 }
                });

        /// <summary>
        /// Active transactions gauge
        /// </summary>
        private static readonly Gauge ActiveTransactions = Metrics
            .CreateGauge("active_transactions", "Number of transactions currently being processed",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "transaction_type" }
                });

        /// <summary>
        /// Back4App API latency in milliseconds
        /// </summary>
        private static readonly Histogram Back4AppLatencyMs = Metrics
            .CreateHistogram("back4app_api_latency_ms", "Back4App API call latency in milliseconds",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "operation" },
                    Buckets = new double[] { 50, 100, 250, 500, 1000, 2500, 5000, 10000 }
                });

        /// <summary>
        /// Back4App API errors counter
        /// </summary>
        private static readonly Counter Back4AppErrors = Metrics
            .CreateCounter("back4app_errors_total", "Total number of Back4App API errors",
                new CounterConfiguration
                {
                    LabelNames = new[] { "operation", "error_code" }
                });

        // Helper methods for RED metrics

        /// <summary>
        /// Track HTTP request
        /// </summary>
        public void RecordHttpRequest(string method, string path, int statusCode, double durationSeconds)
        {
            var labels = new[] { method, path, statusCode.ToString() };
            HttpRequestsTotal.WithLabels(labels).Inc();
            HttpRequestDuration.WithLabels(labels).Observe(durationSeconds);
        }

        /// <summary>
        /// Track HTTP error
        /// </summary>
        public void RecordHttpError(string method, string path, string errorType)
        {
            HttpErrors.WithLabels(new[] { method, path, errorType }).Inc();
        }

        // Helper methods for business metrics

        /// <summary>
        /// Track transaction creation
        /// </summary>
        public void RecordTransaction(string transactionType, string status)
        {
            TransactionsTotal.WithLabels(new[] { transactionType, status }).Inc();
        }

        /// <summary>
        /// Track failed transfer
        /// </summary>
        public void RecordFailedTransfer(string reason)
        {
            FailedTransfersTotal.WithLabels(new[] { reason }).Inc();
        }

        /// <summary>
        /// Record balance check latency
        /// </summary>
        public void RecordBalanceCheckLatency(string accountId, double latencyMs)
        {
            BalanceCheckLatencyMs.WithLabels(new[] { accountId }).Observe(latencyMs);
        }

        /// <summary>
        /// Increment active transactions
        /// </summary>
        public void IncrementActiveTransactions(string transactionType)
        {
            ActiveTransactions.WithLabels(new[] { transactionType }).Inc();
        }

        /// <summary>
        /// Decrement active transactions
        /// </summary>
        public void DecrementActiveTransactions(string transactionType)
        {
            ActiveTransactions.WithLabels(new[] { transactionType }).Dec();
        }

        /// <summary>
        /// Record Back4App API latency
        /// </summary>
        public void RecordBack4AppLatency(string operation, double latencyMs)
        {
            Back4AppLatencyMs.WithLabels(new[] { operation }).Observe(latencyMs);
        }

        /// <summary>
        /// Record Back4App API error
        /// </summary>
        public void RecordBack4AppError(string operation, string errorCode)
        {
            Back4AppErrors.WithLabels(new[] { operation, errorCode }).Inc();
        }
    }
}
