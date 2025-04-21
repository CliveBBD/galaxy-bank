using Newtonsoft.Json;
using System.Diagnostics;
using Cli.Models;
using Microsoft.Extensions.Configuration;
using Cli.Helpers;

namespace Cli.Services;
 
public class AuthService
{
    private readonly HttpClient _httpClient;
    // API base URL (this should match your Web API's address)
    private readonly string _apiBaseUrl;

    public AuthService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token);
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        _apiBaseUrl = Constants.ApiBaseUrl;
    }
 
    public async Task<LoginResult> LoginAsync()
    {
        try 
        {
            return await PollForToken();
        }
        catch (InvalidOperationException)
        {
            return new LoginResult
            {
                Success = false,
                Token = new Token { IdToken = "", SessionId = "", Role = "" }
            };
        }
    }

    public async Task<LoginResult> PollForToken()
    {
        Token token = new() { IdToken = "", Role = "", SessionId = "" };
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/login");
        response.EnsureSuccessStatusCode();
 
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
        if(loginResponse != null)
        {
            OpenBrowser(loginResponse.AuthUrl);
            CliWidgets.RenderWarning("A browser window has been opened. Please complete the authentication process there.\nWaiting for authentication to complete...");
            token = await PollForTokenAsync(loginResponse.SessionId);
        }
    
        return new LoginResult
        {
            Success = token != null,
            Token = token ?? new Token() { IdToken = "", Role = "", SessionId = "" }
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
 
    private async Task<Token> PollForTokenAsync(string sessionId)
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
                    var tokenResponse = JsonConvert.DeserializeObject<Token>(content);
                    return new Token()
                    {
                        IdToken = tokenResponse != null ? tokenResponse.IdToken : "",
                        SessionId = sessionId,
                        Role = tokenResponse != null ? tokenResponse.Role : ""
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
        return new Token() { IdToken = "", Role = "", SessionId = "" };
    }

    public async Task<HttpResponseMessage> LogoutAsync(string sessionId)
    {   
        var logoutRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        { ["sessionId"] = sessionId });
        return await _httpClient.PostAsync($"{_apiBaseUrl}/logout", logoutRequest);
    }   
}
