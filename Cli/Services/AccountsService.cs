namespace Cli.Services;

public class AccountsService
{
    private readonly HttpClient _httpClient;

    public AccountsService()
    {
        _httpClient = new HttpClient();
    }
    public async Task<HttpResponseMessage> GetAccountTypes()
    {
        var accountTypes = await _httpClient.GetAsync("https://localhost:7059/accounts/account-types");
        accountTypes.EnsureSuccessStatusCode();
        return accountTypes;
    }
}