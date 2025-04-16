namespace Cli.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int TransactionReferenceID { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; } = new TransactionType();
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}