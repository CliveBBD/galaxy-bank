using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class AccountType
    {
        [Required]
        public required int AccountTypeId { get; set; }
        [Required]
        public required string Name { get; set; }

        public static explicit operator int(AccountType? v)
        {
            throw new NotImplementedException();
        }
    }
}