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
        private readonly string myBaseUrl;
        private readonly string myAppId;
        private readonly string myApiKey;

        public Back4AppService(HttpClient httpClient, IConfiguration configuration)
        {
            myHttpClient = httpClient;
            myBaseUrl = GetConfigurationValue("Back4App:BaseUrl", configuration);
            myAppId = GetConfigurationValue("Back4App:AppId", configuration);
            myApiKey = GetConfigurationValue("Back4App:RestApiKey", configuration);
        }

        public async Task<TransactionResponse> GetTransaction(int transactionId)
        {
            var result = await GetTransactionByField("txnId", transactionId);
            return result;
        }

        public async Task<TransactionResponse> CreateTransaction(CreateTransactionRequest request)
        {
            var existing = await GetTransactionByField("idempotencyKey", request.IdempotencyKey);

            if (existing != null)
            {
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

            return await GetTransactionByField("objectId", created.ObjectId);
        }

        public async Task<TransactionResponse> UpdateTransaction(int txnId, UpdateTransactionRequest request)
        {
            var existing = await GetTransactionByField("txnId", txnId);

            if (existing == null)
            {
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

        private async Task<string> GetObjectIdByField(string field, object value)
        {
            var whereObj = new JsonObject { [field] = JsonValue.Create(value) };
            var encoded = WebUtility.UrlEncode(whereObj.ToString());

            var result = await GetAsync<ParseResponse<CreatedDto>>($"/classes/Transaction?where={encoded}");

            return result?.Results?.FirstOrDefault()?.ObjectId;
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var request = GetHttpRequest(HttpMethod.Get, myBaseUrl + url);
            var response = await myHttpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        private async Task<T> PostAsync<T>(string url, object body)
        {
            var request = GetHttpRequest(HttpMethod.Post, myBaseUrl + url);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await myHttpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        private async Task PutAsync(string url, object body)
        {
            var request = GetHttpRequest(HttpMethod.Put, myBaseUrl + url);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            await myHttpClient.SendAsync(request);
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