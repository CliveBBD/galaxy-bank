using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class AccountCreateRequest
    {

        public string AccountTypeName { get; set; } = String.Empty;

    }
}