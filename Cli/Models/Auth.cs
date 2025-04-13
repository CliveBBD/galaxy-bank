namespace Cli.Models;

public class Token

{
    public string? AccessToken { get; set; }

    public string? IdToken { get; set;}

    public DateTime ExpiresAt { get; set; }

    public string? SessionId { get; set; }
 
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public class LoginResponse
{
    public string? AuthUrl { get; set; }
    public string? SessionId { get; set; }
}
 
public class TokenResponse
{
    public string? AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? IdToken { get; set; }
}

public class RefreshResponse
{
    public string? AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? IdToken { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public Token? Token { get; set; }
}