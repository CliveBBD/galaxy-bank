using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class AccountStatement
    {
        [Required]
        public required int AccountID { get; set; }
        [Required]
        public required DateTime StatementPeriodStart { get; set; }
        [Required]
        public required DateTime StatementPeriodEnd { get; set; }
        [Required]
        public required DateTime GeneratedAt { get; set; }
        [Required]
        public required int OpeningBalance { get; set; }
        [Required]
        public required int ClosingBalance { get; set; }
        [Required]
        public required IEnumerable<Transaction> Transactions { get; set; }
    }
}