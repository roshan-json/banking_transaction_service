using banking_transaction_service.Models;
using banking_transaction_service.Models.Dtos;
using System.Text.Json;

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

        public async Task<Transaction> GetTransaction(int transactionId)
        {
            var whereClause = JsonSerializer.Serialize(new { txnId = transactionId });
            var encodedWhere = System.Net.WebUtility.UrlEncode(whereClause);
            var request = GetHttpRequest(HttpMethod.Get, myBaseUrl + $"/classes/Transaction?where={encodedWhere}");
            
            var response = await myHttpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ParseResponse<TransactionDto>>(json);
            if(parsed?.Results is null || parsed.Results.Count == 0)
            {
                return null;
            }

            var result = parsed.Results.First();
            var transaction = new Transaction
            {
                Id = result.Id,
                Type = result.Type,
                AccountId = result.AccountId,
                Amount = result.Amount,
                CounterParty = result.CounterParty,
                Reference = result.Reference,
                CreatedAt = result.CreatedAt
            };

            return transaction;
        }

        private HttpRequestMessage GetHttpRequest(HttpMethod httpMethod, string url)
        {
            var request = new HttpRequestMessage(httpMethod, url);
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
            if(configuration.GetValue<string>(key) is string value)
            {
                return value;
            }
            
            throw new Exception($"Configuration value for {key} not found.");
        }
    }
}