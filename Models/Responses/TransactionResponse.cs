using System.Text.Json.Serialization;

namespace banking_transaction_service.Models
{
    /// <summary>
    /// Response returned for transaction retrieval, creation, and updates.
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>Internal transaction identifier.</summary>
        public int Id { get; set; }

        /// <summary>Transaction type, for example debit or credit.</summary>
        public string Type { get; set; }

        /// <summary>The account id associated with this transaction.</summary>
        public int AccountId { get; set; }

        /// <summary>The posted transaction amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>The counterparty involved in the transaction.</summary>
        public string CounterParty { get; set; }

        /// <summary>Reference or memo text associated with the transaction.</summary>
        public string Reference { get; set; }

        /// <summary>The timestamp when the transaction was created.</summary>
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public string IdempotencyKey { get; set; }
    }
}
