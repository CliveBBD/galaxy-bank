using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class StatementRequest
    {
        [Required]
        public required DateTime StartDate { get; set; }
        [Required]
        public required DateTime EndDate { get; set; }
    }
}