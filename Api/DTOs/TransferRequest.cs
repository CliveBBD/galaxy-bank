using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class TransferRequest
    {
        [Required]
        public required int FromAccountID { get; set; }
        [Required]
        public required int ToAccountID { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The amount must be a positive.")]
        public required int Amount { get; set; }
        [StringLength(50, ErrorMessage = "{0} length must be between {2} and {3}.", MinimumLength = 1)]
        public string FromReference { get; set; } = "Transfer";
        public string ToReference { get; set; } = "Transfer";
    }
}