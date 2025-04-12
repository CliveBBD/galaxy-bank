using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UserUpdateRoleRequest
    {
        [Required]
        public required int RoleID { get; set; }
    }
}