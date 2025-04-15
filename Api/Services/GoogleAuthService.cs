using Google.Apis.Auth.OAuth2.Responses;
using System.Security.Cryptography;
using Api.Models;
using Newtonsoft.Json;
using Namotion.Reflection;

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
    private readonly string? AuthorizationEndpoint;
    private readonly string? TokenEndpoint;
    private readonly string? TokenInfoEndpoint;
 
    // Define the required scopes
    private readonly string[]? _scopes;
 
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
        _scopes = [
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/userinfo.profile"
            ];

        AuthorizationEndpoint = _configuration["Authentication:AuthorizationEndpoint"];
        TokenEndpoint = _configuration["Authentication:TokenEndpoint"];
        TokenInfoEndpoint = _configuration["Authentication:TokenInfoEndpoint"];
    }
 
    public string GenerateAuthUrl(string sessionId)
    {
        // Generate a state parameter to prevent CSRF
        var state = sessionId;
 
        // Build the authorization URL
        var authorizationUrl = $"{AuthorizationEndpoint}?" +
            $"client_id={_clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(_redirectUri ?? "")}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", _scopes ?? []))}&" +
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
            ["client_id"] = _clientId ?? "",
            ["client_secret"] = _clientSecret ?? "",
            ["redirect_uri"] = _redirectUri ?? "",
            ["grant_type"] = "authorization_code",
        });
 
        // Make the token request
        var response = await _httpClient.PostAsync(TokenEndpoint, tokenRequest);
        response.EnsureSuccessStatusCode();
 
        // Parse the token response
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
 
        var token = new StoredToken
        {
            IdToken = tokenResponse != null ? tokenResponse.IdToken: "",
            AccessToken = tokenResponse != null ? tokenResponse.AccessToken : "",
            RefreshToken = tokenResponse != null ? tokenResponse.RefreshToken : "",
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
            ["client_id"] = _clientId ?? "",
            ["client_secret"] = _clientSecret ?? "",
            ["grant_type"] = "refresh_token"
        });
 
        // Make the refresh token request
        var response = await _httpClient.PostAsync(TokenEndpoint, refreshRequest);
        response.EnsureSuccessStatusCode();
 
        // Parse the token response
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
 
        var token = new StoredToken
        {   
            IdToken = tokenResponse != null ? tokenResponse.IdToken : "",
            AccessToken = tokenResponse != null ? tokenResponse.AccessToken : "",
            RefreshToken = refreshToken, // The refresh token doesn't change
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.GetValueOrDefault())
        };
 
        return token;
    }
 
    public string GenerateSessionId()
    {
        using var randomNumberGenerated = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        randomNumberGenerated.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public bool IsNotNull(object obj)
    {
        return obj != null;
    }

    public async Task<bool> IsValidToken(string jwt)
    {
        string verifyRequestUrl = $"{TokenInfoEndpoint}?id_token={jwt}";
        var tokenVerification = await _httpClient.GetAsync(verifyRequestUrl);
        tokenVerification.EnsureSuccessStatusCode();
        var tokenVerificationContent = await tokenVerification.Content.ReadAsStringAsync();
        var json = JsonConvert.DeserializeObject(tokenVerificationContent);
        return !json.HasProperty("error");
    }
}