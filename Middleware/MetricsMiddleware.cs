using banking_transaction_service.Services;

namespace banking_transaction_service.Middleware
{
    /// <summary>
    /// Middleware to track HTTP request metrics (RED metrics)
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate myNext;
        private readonly MetricsService myMetricsService;

        public MetricsMiddleware(RequestDelegate next, MetricsService metricsService)
        {
            myNext = next;
            myMetricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "unknown";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await myNext(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var durationSeconds = stopwatch.Elapsed.TotalSeconds;

                // Record metrics
                myMetricsService.RecordHttpRequest(method, path, statusCode, durationSeconds);

                // Record errors
                if (statusCode >= 400)
                {
                    var errorType = statusCode switch
                    {
                        400 => "bad_request",
                        404 => "not_found",
                        500 => "internal_error",
                        502 => "bad_gateway",
                        503 => "service_unavailable",
                        _ => $"error_{statusCode}"
                    };

                    myMetricsService.RecordHttpError(method, path, errorType);
                }
            }
        }
    }
}
