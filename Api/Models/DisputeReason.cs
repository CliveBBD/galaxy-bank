using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class DisputeReason
    {
        [Required]
        public required int DisputeReasonID { get; set; }
        [Required]
        public required string Description { get; set; }
    }
}