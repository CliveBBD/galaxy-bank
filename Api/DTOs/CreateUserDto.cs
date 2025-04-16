using Api.Models;

public class CreateUserDto
{
    // public required int UserID { get; set; }
    public required string GoogleID { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string RoleName { get; set; }
}
