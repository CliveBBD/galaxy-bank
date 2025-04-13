using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class RedactedUser
    {
        [Required]
        public required int UserID { get; set; }
        [Required]
        public required string Username { get; set; }
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}