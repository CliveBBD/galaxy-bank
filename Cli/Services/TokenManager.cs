using Newtonsoft.Json;
namespace Cli.Services;
 
public class Token

{

    public string AccessToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string SessionId { get; set; }
 
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

}
 
public class TokenManager

{

    private readonly string _configPath;

    private Token _cachedToken;
 
    public TokenManager()

    {

        _configPath = Path.Combine(

            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),

            "GoogleAuthCli",

            "token.json");

    }
 
    public async Task SaveTokenAsync(Token token)

    {

        _cachedToken = token;
 
        // Ensure directory exists

        var directory = Path.GetDirectoryName(_configPath);

        if (!Directory.Exists(directory))

        {

            Directory.CreateDirectory(directory);

        }
 
        // Save token to file

        var json = JsonConvert.SerializeObject(token);

        await File.WriteAllTextAsync(_configPath, json);
 
        Console.WriteLine("Token saved successfully.");

    }
 
    public async Task<Token?> GetTokenAsync()

    {

        if (_cachedToken != null)

        {

            return _cachedToken;

        }

        if (!File.Exists(_configPath))

        {

            return null;

        }
 
        // Load token from file

        var json = await File.ReadAllTextAsync(_configPath);

        var token = JsonConvert.DeserializeObject<Token>(json);

        _cachedToken = token;
 
        return token;

    }
 
    public async Task ClearTokenAsync()

    {

        _cachedToken = null;

        if (File.Exists(_configPath))

        {

            File.Delete(_configPath);

        }

        await Task.CompletedTask;

    }

}
 