namespace Cli.Models;

public class Token

{
    public required string AccessToken { get; set; }

    public required string IdToken { get; set;}

    public DateTime ExpiresAt { get; set; }

    public required string SessionId { get; set; }
 
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public class LoginResponse
{
    public required string AuthUrl { get; set; }
    public required string SessionId { get; set; }
}
 
public class TokenResponse
{
    public required string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string IdToken { get; set; }
}

public class RefreshResponse
{
    public required string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string IdToken { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public required Token Token { get; set; }
}