namespace banking_transaction_service.Models.Requests
{
    /// <summary>
    /// Request payload used to create a new banking transaction.
    /// </summary>
    public class CreateTransactionRequest
    {
        /// <summary>Transaction category such as debit or credit.</summary>
        public string Type { get; set; }

        /// <summary>The account id associated with the transaction.</summary>
        public int AccountId { get; set; }

        /// <summary>The transaction amount in the account currency.</summary>
        public decimal Amount { get; set; }

        /// <summary>The counterparty for the transaction.</summary>
        public string CounterParty { get; set; }

        /// <summary>Customer-facing reference or memo text.</summary>
        public string Reference { get; set; }

        /// <summary>A unique idempotency key to prevent duplicate transaction creation.</summary>
        public string IdempotencyKey { get; set; }
    }
}
