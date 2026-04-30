using System.Text.Json.Serialization;

namespace banking_transaction_service.Models
{
    public class ParseResponse<T> where T : class
    {
        [JsonPropertyName("results")]
        public List<T> Results { get; set; }
    }
}
