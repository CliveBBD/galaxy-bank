using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class DailyTransactionReport
    {
        [Required]
        public required DateTime ReportDate { get; set; }
        [Required]
        public required int TotalTransactions { get; set; }
        [Required]
        public required int TotalVolumne { get; set; }
        [Required]
        public required IEnumerable<VolumeByType> VolumeByTypes { get; set; } 

        public sealed class VolumeByType
        {
            [Required]
            public required TransactionType TransactionType { get; set; }
            [Required]
            public required int Count { get; set; }
            [Required]
            public required int TotalAmount { get; set; }
        }
    }
}