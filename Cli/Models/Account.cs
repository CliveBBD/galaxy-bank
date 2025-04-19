namespace Cli.Models
{
    public class Account
    {
        public int UserId { get; set; }
        public required AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string AccountNumber { get; set; }
    }

    public class AccountType
    {
        public int AccountTypeId { get; set; }
        public required string Name { get; set; }
    }
}