namespace Api.Models;

public class StoredToken
{
    public required string IdToken { get; set; } 
    public required string Role { get; set; }
    public required string SessionId { get; set; }
}

public class LogOut
{
    public string? Error = string.Empty;
    public string? Message = string.Empty;
}