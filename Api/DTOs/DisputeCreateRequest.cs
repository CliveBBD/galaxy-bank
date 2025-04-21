using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class DisputeCreateRequest
    {
        [Required]
        public required int DisputedTransactionReferenceID { get; set; }
        [Required]
        public required string Details { get; set; }
        [Required]
        public required int DisputeReasonId { get; set; }
    }
}