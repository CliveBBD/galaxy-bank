using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class WithdrawRequest
    {
        [Required]
        public required int AccountId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The amount must be a positive.")]
        public required int Amount { get; set; }
        public string Reference { get; set; } = "Deposit";
    }
}