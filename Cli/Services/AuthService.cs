using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
 
namespace Cli.Services;
 
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;
 
    // API base URL (this should match your Web API's address)
    private readonly string _apiBaseUrl = "https://localhost:7059";
 
    public AuthService(TokenManager tokenManager)
    {
        _httpClient = new HttpClient();
        _tokenManager = tokenManager;
    }
 
    private async Task<Token> GetValidTokenAsync()
    {
        var token = await _tokenManager.GetTokenAsync();
 
        if (token == null)
        {
            throw new InvalidOperationException("Not authenticated. Please run 'login' command first.");
        }
 
        // Check if token is expired
        if (token.IsExpired)
        {
            // Refresh the token
            token = await RefreshTokenAsync(token.SessionId);
            await _tokenManager.SaveTokenAsync(token);
        }
 
        return token;
    }
 
    private async Task<Token?> RefreshTokenAsync(string sessionId)
    {
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/refresh/{sessionId}", null);
        response.EnsureSuccessStatusCode();
 
        var responseContent = await response.Content.ReadAsStringAsync();
        var refreshResponse = JsonConvert.DeserializeObject<RefreshResponse>(responseContent);
 
        return new Token
        {
            IdToken = refreshResponse.IdToken,
            AccessToken = refreshResponse.AccessToken,
            ExpiresAt = refreshResponse.ExpiresAt,
            SessionId = sessionId
        };
    }
 
    public async Task<LoginResult> LoginAsync()
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/login");
        response.EnsureSuccessStatusCode();
 
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
        OpenBrowser(loginResponse.AuthUrl);
 
        Console.WriteLine("A browser window has been opened. Please complete the authentication process there.");
        Console.WriteLine("Waiting for authentication to complete...");
 
        var token = await PollForTokenAsync(loginResponse.SessionId);
 
        return new LoginResult
        {
            Success = token != null,
            Token = token
        };
    }
 
    private void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                Process.Start("xdg-open", url); // Linux
            }
            catch
            {
                try
                {
                    Process.Start("open", url); // macOS
                }
                catch
                {
                    Console.WriteLine($"Could not open browser automatically. Please open this URL manually: {url}");
                }
            }
        }
    }
 
    private async Task<Token?> PollForTokenAsync(string sessionId)
    {
        int maxAttempts = 30;
        int attempts = 0;
 
        while (attempts < maxAttempts)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/token/{sessionId}");
 
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);
 
                    return new Token
                    {
                        IdToken = tokenResponse.IdToken,
                        AccessToken = tokenResponse.AccessToken,
                        ExpiresAt = tokenResponse.ExpiresAt,
                        SessionId = sessionId
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error polling for token: {ex.Message}");
            }
 
            attempts++;
            await Task.Delay(2000);
        }
 
        return null;
    }
 
    public async Task<string> GetProfileAsync()
    {
        var token = await GetValidTokenAsync();
 
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/profile");
        response.EnsureSuccessStatusCode();
 
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
 
    private class LoginResponse
    {
        public string? AuthUrl { get; set; }
        public string? SessionId { get; set; }
    }
 
    private class TokenResponse
    {
        public string? AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? IdToken { get; set; }
    }
 
    private class RefreshResponse
    {
        public string? AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? IdToken { get; set; }
    }
}
 
public class LoginResult
{
    public bool Success { get; set; }
    public Token? Token { get; set; }
}