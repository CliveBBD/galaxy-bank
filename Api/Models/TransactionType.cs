using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class TransactionType
    {
        [Required]
        public required int TransactionTypeID { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}