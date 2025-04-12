using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class NetWorth
    {
        [Required]
        public required int TotalAssets { get; set; }
        [Required]
        public required int TotalLiabilities { get; set; }
        [Required]
        public required int TotalWorth { get; set; }
        [Required]
        public required DateTime CalculationTimestamp { get; set; }

    }
}