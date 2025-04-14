namespace Api.Models;

public class StoredToken

{

    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public required string IdToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

}