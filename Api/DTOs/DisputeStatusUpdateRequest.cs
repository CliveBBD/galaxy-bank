using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class DisputeStatusUpdateRequest
    {
        [Required]
        public required int DisputeID { get; set; }
        [Required]
        public required int NewStatusId { get; set; }
    }
}