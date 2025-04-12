using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class RedactedTransaction
    {
        [Required]
        public required int TransactionID { get; set; }
        [Required]
        public required int TransactionReferenceID { get; set; }

        [Required]
        public required int AccountID { get; set; }
        [Required]
        public required int Amount { get; set; }
        [Required]
        public required TransactionType TransactionType { get; set; }
        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}