using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs
{
    public class TransactionRequest
    {
        [Required]
        public required int TransactionID { get; set; }
        [Required]
        public required int TransactionReferenceID { get; set; }
        [Required]
        public required string Reference { get; set; }

        [Required]
        public required int Amount { get; set; }
        [Required]
        public required TransactionType TransactionType { get; set; }
        [Required]
        public required int BalanceAfterTransaction { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; }
        [Required]
        public required string AccountNumber { get; set; }
    }
}