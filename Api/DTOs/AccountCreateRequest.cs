using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class AccountCreateRequest
    {
        [Required]
        public required int UserId { get; set; }

        [JsonConverter(typeof(CaseInsensitiveEnumConverter<AccountType>))]
        public AccountType AccountType  { get; set; } 

        public  int Balance { get; set; } = 50;
    }
}