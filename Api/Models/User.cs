using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class User
    {
        [Required]
        public required int UserID { get; set; }
        [Required]
        public required string GoogleID { get; set; }
        [Required]
        public required string Username { get; set; }
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
        [Required]
        public required Role Role { get; set; }
    }
}