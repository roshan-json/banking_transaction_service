namespace banking_transaction_service.Models.Requests
{
    public class UpdateTransactionRequest
    {
        public int? AccountId { get; set; }
        public decimal? Amount { get; set; }
        public string? Type { get; set; }
        public string? CounterParty { get; set; }
        public string? Reference { get; set; }
    }
}
