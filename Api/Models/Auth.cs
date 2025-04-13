namespace Api.Models;

public class StoredToken

{

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? IdToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

}