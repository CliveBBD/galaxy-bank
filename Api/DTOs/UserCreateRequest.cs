using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UserCreateRequest
    {
        [Required]
        public required string GoogleIDToken { get; set; }
    }
}