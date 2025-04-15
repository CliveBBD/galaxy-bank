using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class DisputeHistoryEntry
    {
        [Required]
        public required int DisputeHistoryID { get; set; }
        [Required]
        public required int DisputeID { get; set; }
        [Required]
        public required DisputeStatus Status { get; set; }
        [Required]
        public required DateTime UpdatedAt { get; set; }
        [Required]
        public required RedactedUser UpdatedBy { get; set; }
    }
}