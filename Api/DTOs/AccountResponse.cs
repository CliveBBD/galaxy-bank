using Api.Models;
public class AccountResponse
{
    public int AccountId { get; set; }
    public int UserId { get; set; }
    public AccountType AccountType { get; set; } = default!;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AccountNumber { get; set; }

}
