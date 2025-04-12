using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Account
    {
        [Required]
        public required int AccountID { get; set; }
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required AccountType AccountType { get; set; }
        [Required]
        public required int Balance { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}