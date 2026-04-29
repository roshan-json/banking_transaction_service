using System.Net.Http;
using System.Text;
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
            myBaseUrl = configuration["Back4App:BaseUrl"];
            myAppId = configuration["Back4App:AppId"];
            myApiKey = configuration["Back4App:RestApiKey"];
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-Parse-Application-Id", myAppId);
            request.Headers.Add("X-Parse-REST-API-Key", myApiKey);
        }

        public async Task<string> CreateTransaction(object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://parseapi.back4app.com/classes/transactions");

            AddHeaders(request);

            request.Content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json"
            );

            var response = await myHttpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetTransactionById(string objectId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{myBaseUrl}/classes/transactions/{objectId}");

            AddHeaders(request);

            var response = await myHttpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}