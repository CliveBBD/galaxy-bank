using Api.Models;

public class CreateUserDto(string googleID, string username, string email)
{
    // public required int UserID { get; set; }
    public string GoogleID { get; set; } = googleID;
    public string Username { get; set; } = username;
    public string Email { get; set; } = email;
    public string RoleName { get; set; }
}
