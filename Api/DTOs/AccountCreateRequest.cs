using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class AccountCreateRequest
    {
        [Required]
        public required int UserId { get; set; }

        public string AccountTypeName { get; set; } = String.Empty;

    }
}