using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class DisputeCreateRequest
    {
        [Required]
        public required int DisputedTransactionReferenceID { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "{0} length must be between {2} and {3}.", MinimumLength = 10)]
        public required string Reason { get; set; }
    }
}