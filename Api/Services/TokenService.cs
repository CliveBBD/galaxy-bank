namespace Api.Services;
 
public class TokenService

{

    private readonly Dictionary<string, StoredToken> _tokenStore = new();
 
    public void StoreToken(string userKey, StoredToken token)

    {

        if (_tokenStore.ContainsKey(userKey))

        {

            _tokenStore[userKey] = token;

        }

        else

        {

            _tokenStore.Add(userKey, token);

        }

    }
 
    public StoredToken GetToken(string userKey)

    {

        return _tokenStore.TryGetValue(userKey, out var token) ? token : null;

    }
 
    public void RemoveToken(string userKey)

    {

        if (_tokenStore.ContainsKey(userKey))

        {

            _tokenStore.Remove(userKey);

        }

    }

}
 
public class StoredToken

{

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public string IdToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

}
 
 
