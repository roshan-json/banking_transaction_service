namespace banking_transaction_service.Models
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string CounterParty { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IdempotencyKey { get; set; }
    }
}
