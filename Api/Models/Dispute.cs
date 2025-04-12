using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Dispute
    {
        [Required]
        public required int DisputeID { get; set; }
        [Required]
        public required string Reason { get; set; }
        [Required]
        public required int DisputedTransactionReferenceID { get; set; }
        [Required]
        public required DisputeStatus CurrentStatus { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; }

    }
}