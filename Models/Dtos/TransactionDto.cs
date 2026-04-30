using System.Text.Json.Serialization;

namespace banking_transaction_service.Models.Dtos
{
    public class TransactionDto
    {
        [JsonPropertyName("objectId")]
        public string ObjectId { get; set; }

        [JsonPropertyName("txnId")]
        public int Id { get; set; }

        [JsonPropertyName("txnType")]
        public string Type { get; set; }

        [JsonPropertyName("accountId")]
        public int AccountId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("counterParty")]
        public string CounterParty { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
