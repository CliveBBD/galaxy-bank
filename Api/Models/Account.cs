using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Account
    {
        [Required]
        public required int AccountId { get; set; }
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int AccountTypeId { get; set; }
        [Required]
        public required int Balance { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.Now;


    }
}