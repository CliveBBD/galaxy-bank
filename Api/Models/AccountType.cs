using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class AccountType
    {
        [Required]
        public required int AccountTypeID { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}