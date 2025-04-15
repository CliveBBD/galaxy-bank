namespace Cli.Models;

public class Token

{
    public required string IdToken { get; set;}
    public required string SessionId { get; set; }
}

public class LoginResponse
{
    public required string AuthUrl { get; set; }
    public required string SessionId { get; set; }
}
 
public class TokenResponse
{
    public required string IdToken { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public required Token Token { get; set; }
}