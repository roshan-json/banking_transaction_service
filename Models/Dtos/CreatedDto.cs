using System.Text.Json.Serialization;

namespace banking_transaction_service.Models.Dtos
{
    public class CreatedDto
    {
        [JsonPropertyName("objectId")]
        public string ObjectId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
