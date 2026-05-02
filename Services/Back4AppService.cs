using banking_transaction_service.Models;
using banking_transaction_service.Models.Dtos;
using banking_transaction_service.Models.Requests;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace banking_transaction_service.Services
{
    public class Back4AppService
    {
        private readonly HttpClient myHttpClient;
        private readonly ILogger<Back4AppService> myLogger;
        private readonly MetricsService myMetricsService;
        private readonly string myBaseUrl;
        private readonly string myAppId;
        private readonly string myApiKey;

        public Back4AppService(HttpClient httpClient, ILogger<Back4AppService> logger, IConfiguration configuration, MetricsService metricsService)
        {
            myHttpClient = httpClient;
            myLogger = logger;
            myMetricsService = metricsService;
            myBaseUrl = GetConfigurationValue("Back4App:BaseUrl", configuration);
            myAppId = GetConfigurationValue("Back4App:AppId", configuration);
            myApiKey = GetConfigurationValue("Back4App:RestApiKey", configuration);
        }

        public async Task<TransactionResponse> GetTransaction(int transactionId)
        {
            myLogger.LogInformation($"Fetching transaction with transactionId: {transactionId}");
            var result = await GetTransactionByField("txnId", transactionId);
            return result;
        }

        public async Task<List<TransactionResponse>> GetTransactions(int accountId)
        {
            myLogger.LogInformation($"Fetching transactions from accountId: {accountId}");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var result = await GetTransactionsByField("accountId", accountId);
                return result;
            }
            finally
            {
                stopwatch.Stop();
                myMetricsService.RecordBalanceCheckLatency(accountId.ToString(), stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public async Task<TransactionResponse> CreateTransaction(CreateTransactionRequest request)
        {
            myLogger.LogInformation($"Creating a new transaction");
            myMetricsService.IncrementActiveTransactions(request.Type);

            try
            {
                var existing = await GetTransactionByField("idempotencyKey", request.IdempotencyKey);

                if (existing != null)
                {
                    myLogger.LogWarning($"Duplicate transaction was found with the same key");
                    myMetricsService.RecordTransaction(request.Type, "duplicate");
                    return existing;
                }

                var payload = new
                {
                    txnId = await GetNextTxnId(),
                    txnType = request.Type,
                    accountId = request.AccountId,
                    amount = request.Amount,
                    counterParty = request.CounterParty,
                    reference = request.Reference,
                    idempotencyKey = request.IdempotencyKey
                };

                var created = await PostAsync<CreatedDto>("/classes/Transaction", payload);
                var result = await GetTransactionByField("objectId", created.ObjectId);

                myMetricsService.RecordTransaction(request.Type, "success");
                return result;
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to create transaction");
                myMetricsService.RecordTransaction(request.Type, "failed");
                myMetricsService.RecordFailedTransfer(ex.GetType().Name);
                throw;
            }
            finally
            {
                myMetricsService.DecrementActiveTransactions(request.Type);
            }
        }

        public async Task<TransactionResponse> UpdateTransaction(int txnId, UpdateTransactionRequest request)
        {
            var existing = await GetTransactionByField("txnId", txnId);

            if (existing == null)
            {
                myLogger.LogWarning($"Transaction could not be found for update");
                return null;
            }

            var objectId = await GetObjectIdByField("txnId", txnId);
            var payload = GetDynamicPayload(request);

            await PutAsync($"/classes/Transaction/{objectId}", payload);

            return await GetTransactionByField("objectId", objectId);
        }

        private async Task<TransactionResponse> GetTransactionByField(string field, object value)
        {
            var whereObj = new JsonObject { [field] = JsonValue.Create(value) };
            var encoded = WebUtility.UrlEncode(whereObj.ToString());

            var result = await GetAsync<ParseResponse<TransactionDto>>($"/classes/Transaction?where={encoded}");

            var dto = result?.Results?.FirstOrDefault();
            if (dto == null)
            {
                return null;
            }

            return Map(dto);
        }

        private async Task<List<TransactionResponse>> GetTransactionsByField(string field, object value)
        {
            var whereObj = new JsonObject { [field] = JsonValue.Create(value) };
            var encoded = WebUtility.UrlEncode(whereObj.ToString());

            var result = await GetAsync<ParseResponse<TransactionDto>>($"/classes/Transaction?where={encoded}");

            var dto = result?.Results;
            if (dto == null)
            {
                return null;
            }

            return Map(dto);
        }

        private async Task<string> GetObjectIdByField(string field, object value)
        {
            var whereObj = new JsonObject { [field] = JsonValue.Create(value) };
            var encoded = WebUtility.UrlEncode(whereObj.ToString());

            var result = await GetAsync<ParseResponse<CreatedDto>>($"/classes/Transaction?where={encoded}");

            return result?.Results?.FirstOrDefault()?.ObjectId;
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var request = GetHttpRequest(HttpMethod.Get, myBaseUrl + url);
                var response = await myHttpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    myMetricsService.RecordBack4AppError("get", response.StatusCode.ToString());
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            finally
            {
                stopwatch.Stop();
                myMetricsService.RecordBack4AppLatency("get", stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private async Task<T> PostAsync<T>(string url, object body)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var request = GetHttpRequest(HttpMethod.Post, myBaseUrl + url);
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await myHttpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    myMetricsService.RecordBack4AppError("post", response.StatusCode.ToString());
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            finally
            {
                stopwatch.Stop();
                myMetricsService.RecordBack4AppLatency("post", stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private async Task PutAsync(string url, object body)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var request = GetHttpRequest(HttpMethod.Put, myBaseUrl + url);
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await myHttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    myMetricsService.RecordBack4AppError("put", response.StatusCode.ToString());
                }
            }
            finally
            {
                stopwatch.Stop();
                myMetricsService.RecordBack4AppLatency("put", stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private TransactionResponse Map(TransactionDto dto)
        {
            return new TransactionResponse
            {
                Id = dto.Id,
                Type = dto.Type,
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                CounterParty = dto.CounterParty,
                Reference = dto.Reference,
                CreatedAt = dto.CreatedAt
            };
        }

        private List<TransactionResponse> Map(List<TransactionDto> dtos)
        {
            var transactionResponses = new List<TransactionResponse>();
            dtos.ForEach(dto =>
            {
                var transactionResponse = new TransactionResponse
                {
                    Id = dto.Id,
                    Type = dto.Type,
                    AccountId = dto.AccountId,
                    Amount = dto.Amount,
                    CounterParty = dto.CounterParty,
                    Reference = dto.Reference,
                    CreatedAt = dto.CreatedAt
                };
                transactionResponses.Add(transactionResponse);
            });

            return transactionResponses;
        }

        private HttpRequestMessage GetHttpRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            AddHeaders(request);
            return request;
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-Parse-Application-Id", myAppId);
            request.Headers.Add("X-Parse-REST-API-Key", myApiKey);
        }

        private string GetConfigurationValue(string key, IConfiguration configuration)
        {
            var value = configuration.GetValue<string>(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Configuration value for {key} not found.");
            }

            return value;
        }

        private async Task<int> GetNextTxnId()
        {
            var result = await GetAsync<ParseResponse<TransactionDto>>("/classes/Transaction?order=-txnId&limit=1");

            var max = result?.Results?.FirstOrDefault()?.Id ?? 0;
            return max + 1;
        }

        private Dictionary<string, object> GetDynamicPayload(UpdateTransactionRequest request)
        {
            var payload = new Dictionary<string, object>();

            if (request.AccountId.HasValue)
                payload["accountId"] = request.AccountId.Value;

            if (request.Amount.HasValue)
                payload["amount"] = request.Amount.Value;

            if (!string.IsNullOrWhiteSpace(request.Type))
                payload["txnType"] = request.Type;

            if (!string.IsNullOrWhiteSpace(request.CounterParty))
                payload["counterParty"] = request.CounterParty;

            if (!string.IsNullOrWhiteSpace(request.Reference))
                payload["reference"] = request.Reference;

            return payload;
        }
    }
}