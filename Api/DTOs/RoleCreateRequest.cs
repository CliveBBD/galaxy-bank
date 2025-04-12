using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class RoleCreateRequest
    {
        [Required]
        [StringLength(50, ErrorMessage = "{0} length must be between {2} and {3}.", MinimumLength = 3)]
        public required string Name { get; set; }
    }
}