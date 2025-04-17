using Google.Apis.Auth.OAuth2.Responses;
using System.Security.Cryptography;
using Api.Models;
using Newtonsoft.Json;
using Namotion.Reflection;
using System.Text.Json;

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

        _clientId = GetValueByKey(Environment.GetEnvironmentVariable("GoogleClientId") ?? _configuration["Authentication:Google:ClientId"], "GoogleClientId");
        _clientSecret = GetValueByKey(Environment.GetEnvironmentVariable("GoogleClientSecret") ?? _configuration["Authentication:Google:ClientSecret"], "GoogleClientSecret");
        _redirectUri = $"https://d11dblihl6n2a9.cloudfront.net/signin-google" ?? _configuration["Authentication:Google:RedirectUri"];
        _scopes = [
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/userinfo.profile"
            ];

        AuthorizationEndpoint = _configuration["Authentication:AuthorizationEndpoint"];
        TokenEndpoint = _configuration["Authentication:TokenEndpoint"];
        TokenInfoEndpoint = _configuration["Authentication:TokenInfoEndpoint"];
    }

    private static string GetValueByKey(string jsonString, string key)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty(key, out JsonElement value))
            {
                return value.ToString();
            }
            else
            {
                return null; // Key not found
            }
        }
        catch (System.Text.Json.JsonException)
        {
            return null; // Invalid JSON
        }
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
            IdToken = tokenResponse != null ? tokenResponse.IdToken: ""
        };
 
        return token ?? new() { IdToken = "" };
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