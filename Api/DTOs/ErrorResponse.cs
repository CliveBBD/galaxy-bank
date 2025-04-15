using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.DTOs
{
    public class ErrorResponse
    {
        private static readonly string DEFAULT_ERROR_MESSAGE = "There was a problem handling your request";
        [Required]
        public string Message { get; set; }
        public string Details { get; set; }

        public ErrorResponse()
        {
            Message = DEFAULT_ERROR_MESSAGE;
            Details = DEFAULT_ERROR_MESSAGE;
        }

        public ErrorResponse(string details)
        {
            Message = DEFAULT_ERROR_MESSAGE;
            Details = details;
        }
        
        public ErrorResponse(string message, string details)
        {
            Message = message;
            Details = details;
        }

        public ErrorResponse(ModelStateDictionary modelState) 
        {
            var errors = modelState
                .Where(state => state.Value.Errors.Count > 0)
                .SelectMany(state => state.Value.Errors.Select(error => $"{state.Key}: {error.ErrorMessage.TrimEnd('.')}.{error.Exception?.Message ?? ""};"))
                .ToList();

            Message = DEFAULT_ERROR_MESSAGE;
            Details = string.Join(" ", errors);
        }

        public ErrorResponse(IEnumerable<ValidationResult> validationResult)
        {
            var errors = validationResult
                .Select(validationError => $"[{string.Join(", ", validationError.MemberNames.Select(member => member.ToString()))}]: {validationError.ErrorMessage};");

            Message = DEFAULT_ERROR_MESSAGE;
            Details = string.Join(" ", errors);
        }
    }
}