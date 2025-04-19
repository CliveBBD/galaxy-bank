using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.DTOs
{
    public class ErrorResponse : ProblemDetails
    {
        private const string DEFAULT_ERROR_TITLE = "There was a problem handling your request";

        public ErrorResponse()
        {
            Title = DEFAULT_ERROR_TITLE;
            Detail = DEFAULT_ERROR_TITLE;
            Status = 400;
        }

        public ErrorResponse(string detail, int statusCode = 400)
        {
            Title = DEFAULT_ERROR_TITLE;
            Detail = detail;
            Status = statusCode;
        }

        public ErrorResponse(string title, string detail, int statusCode = 400)
        {
            Title = title;
            Detail = detail;
            Status = statusCode;
        }

        public ErrorResponse(IEnumerable<ValidationResult> validationResults, int statusCode = 400)
        {
            var errors = validationResults
                .Select(validationError => $"[{string.Join(", ", validationError.MemberNames)}]: {validationError.ErrorMessage};");

            Title = DEFAULT_ERROR_TITLE;
            Detail = string.Join(" ", errors);
            Status = statusCode;
        }

        public ErrorResponse(Exception ex, int statusCode = 500)
        {
            Title = DEFAULT_ERROR_TITLE;
            Detail = ex.Message;
            Status = statusCode;

            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                Extensions["stackTrace"] = ex.StackTrace;
            }
            else
            {
                Extensions["stackTrace"] = "No stack trace available.";
            }

            if (ex.InnerException != null)
            {
                Extensions["innerException"] = ex.InnerException.Message;
            }
            else
            {
                Extensions["innerException"] = "No inner exception.";
            }
        }
    }
}
