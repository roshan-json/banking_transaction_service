namespace banking_transaction_service.Models.Requests
{
    /// <summary>
    /// Request payload used to partially update an existing transaction.
    /// </summary>
    public class UpdateTransactionRequest
    {
        /// <summary>Optional updated account id for the transaction.</summary>
        public int? AccountId { get; set; }

        /// <summary>Optional updated transaction amount.</summary>
        public decimal? Amount { get; set; }

        /// <summary>Optional updated transaction type.</summary>
        public string? Type { get; set; }

        /// <summary>Optional updated counterparty name.</summary>
        public string? CounterParty { get; set; }

        /// <summary>Optional updated reference note.</summary>
        public string? Reference { get; set; }
    }
}
