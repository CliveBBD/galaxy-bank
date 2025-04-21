namespace Cli.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int TransactionReferenceID { get; set; }
        public string Reference { get; set; } = string.Empty;
        public required string AccountNumber { get; set; }
        public int Amount { get; set; }
        public TransactionType TransactionType { get; set; } = new TransactionType();
        public int BalanceAfterTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}