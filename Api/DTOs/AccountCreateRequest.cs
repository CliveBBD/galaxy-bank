using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class AccountCreateRequest
    {
        [Required]
        public required int UserID { get; set; }
        public string AccountTypeName { get; set; } = String.Empty;
    }
}