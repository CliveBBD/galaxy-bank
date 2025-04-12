using Google.Apis.Auth.OAuth2.Responses;
using System.Security.Cryptography;
 
namespace Api.Services;
 
public class GoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly string? _clientId;
    private readonly string? _clientSecret;
    private readonly string? _redirectUri;
 
    // Define the Google OAuth endpoints
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
 
    // Define the required scopes
    private readonly string[] _scopes =
    [
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/userinfo.profile"
    ];
 
    public GoogleAuthService(
        IConfiguration configuration,
        HttpClient httpClient,
        TokenService tokenService)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _tokenService = tokenService;
 
        _clientId = _configuration["Authentication:Google:ClientId"];
        _clientSecret = _configuration["Authentication:Google:ClientSecret"];
        _redirectUri = _configuration["Authentication:Google:RedirectUri"];
    }
 
    public string GenerateAuthUrl(string sessionId)
    {
        // Generate a state parameter to prevent CSRF
        var state = sessionId;
 
        // Build the authorization URL
        var authorizationUrl = $"{AuthorizationEndpoint}?" +
            $"client_id={_clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", _scopes))}&" +
            $"access_type=offline&" +
            $"state={state}&" +
            $"prompt=consent";
 
        return authorizationUrl;
    }
 
    public async Task<StoredToken?> ExchangeCodeForTokenAsync(string code)
    {
        // Set up the token request parameters
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = _redirectUri,
            ["grant_type"] = "authorization_code",
        });
 
        // Make the token request
        var response = await _httpClient.PostAsync(TokenEndpoint, tokenRequest);
        response.EnsureSuccessStatusCode();
 
        // Parse the token response
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(responseContent);
 
        var token = new StoredToken
        {
            IdToken= tokenResponse.IdToken,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.GetValueOrDefault())
        };
 
        return token;
    }
 
    public async Task<StoredToken> RefreshTokenAsync(string refreshToken)
    {
        // Set up the refresh token request parameters
        var refreshRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = !IsNull(_clientId) ? _clientId : "",
            ["client_secret"] = !IsNull(_clientSecret) ? _clientSecret : "",
            ["grant_type"] = "refresh_token"
        });
 
        // Make the refresh token request
        var response = await _httpClient.PostAsync(TokenEndpoint, refreshRequest);
        response.EnsureSuccessStatusCode();
 
        // Parse the token response
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(responseContent);
 
        var token = new StoredToken
        {   
            IdToken = tokenResponse.IdToken,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = refreshToken, // The refresh token doesn't change
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.GetValueOrDefault())
        };
 
        return token;
    }
 
    public string GenerateSessionId()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public bool IsNull(string parameter)
    {
        return parameter == null;
    }
}