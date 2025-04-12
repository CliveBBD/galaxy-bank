using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class TransactionReference
    {
        [Required]
        public required int TransactionReferenceID { get; set; }
    }
}