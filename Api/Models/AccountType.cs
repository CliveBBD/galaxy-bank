using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class AccountType
    {
        [Required]
        public required int AccountTypeId { get; set; }
        [Required]
        public required string Name { get; set; } = string.Empty;

        public static explicit operator int(AccountType? v)
        {
            throw new NotImplementedException();
        }
    }
}