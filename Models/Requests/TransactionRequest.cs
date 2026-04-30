namespace banking_transaction_service.Models.Requests
{
    public class TransactionRequest
    {
        public string Type { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string CounterParty { get; set; }
        public string Reference { get; set; }
        public string IdempotencyKey { get; set; }
    }
}
