using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class DisputeStatusUpdateRequest
    {
        [Required]
        public required int NewStatusId { get; set; }
        [Required]
        public required int UserID { get; set; }
    }
}