using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.Shared;

public class CreateUserDto(string googleID, string username, string email)
{
    public string GoogleID { get; set; } = googleID;
    public string Username { get; set; } = username;
    [Required]
    public string Email { get; set; } = email;
    public string RoleName { get; set; } = Constants.DefaultRoleName;
}
