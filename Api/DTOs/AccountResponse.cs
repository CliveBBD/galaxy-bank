using Api.DTOs;
using Api.Models;

namespace Api.DTOs
{
    public class AccountResponse
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public AccountType AccountType { get; set; } = default!;
        public int Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string AccountNumber { get; set; }

    }
}
