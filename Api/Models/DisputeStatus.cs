using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class DisputeStatus
    {
        [Required]
        public required int DisputeStatusID { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}