using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class ErrorResponse
    {
        [Required]
        public required string Message { get; set; }
        public string? Details { get; set; }
    }
}