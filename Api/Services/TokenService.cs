using Api.Models;

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

    public StoredToken? GetToken(string userKey)
    {
        
        return _tokenStore.TryGetValue(userKey, out var token) ? token : null;
    }
 
    public LogOut RemoveToken(string userKey)
    {
        try 
        {
            _tokenStore.Remove(userKey);
            return new() { Message = "Successfully logged out!"};
        }
        catch(ArgumentNullException e)
        {
            return new() {
                Error = e.Message,  
            };
        }
        
    }
}
 
 
 
