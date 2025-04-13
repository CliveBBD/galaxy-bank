using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class DisputeCreateRequest
    {
        [Required]
        public required int DisputedTransactionReferenceID { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "{0} length must be between {2} and {3}.", MinimumLength = 10)]
        public required string Reason { get; set; }
        public int? UserID { get; set; }
        public string? Email { get; set; }

        public IEnumerable<ValidationResult> Validate()
        {
            var errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Email) && (UserID == 0 || UserID == null))
            {
                errors.Add(new ValidationResult(
                    "Either UserID or Email must be provided.",
                    new[] { nameof(UserID), nameof(Email) }
                ));
            }

            return errors;
        }
    }
}