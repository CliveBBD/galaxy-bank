namespace Cli.Models
{
    public class Account
    {
        public int UserId { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AccountNumber { get; set; }
    }

    public class AccountType
    {
        public int AccountTypeId { get; set; }
        public string Name { get; set; }
    }
}