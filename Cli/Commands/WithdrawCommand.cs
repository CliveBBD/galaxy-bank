using Cli.Helpers;
using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text;
using System.Text.Json;

namespace Cli.Commands
{
    public class WithdrawCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            var reference = CliWidgets.PromptText("Enter a reference for your transaction");
            if (string.IsNullOrEmpty(reference))
            {
                CliWidgets.RenderError("[red]Reference must be specified.[/]");
                return 1;
            }

            if (!int.TryParse(CliWidgets.PromptText("Transfer Amount"), out int amount))
            {
                CliWidgets.RenderError("Enter a valid integer.\nNote that amounts greater than Q 2 147 483 647 can't be withdrawn via the Cli, please visit the nearest galaxy bank.");
                return 1;
            }
            if (amount <= 0)
            {
                CliWidgets.RenderError("[red]Amount must be greater than zero.[/]");
                return 1;
            }
            using var httpClient = new HttpClient();

            var bearerToken = User.Token;
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                var response = httpClient.GetAsync($"{Constants.ApiBaseUrl}/accounts").Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch accounts: {(int)response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                if (accounts == null || !accounts.Any())
                {
                    CliWidgets.RenderWarning("[yellow]No accounts found.[/]");
                    return 1;
                }

                var accountChoices = accounts.Select(a => $"{a.AccountNumber} - {a.AccountType.Name}").ToList();

                var selectedAccount = CliWidgets.RenderSelection("Select an account", accountChoices);

                var accountNumber = selectedAccount.Split(" - ")[0];

                var payload = new
                {
                    AccountNumber = accountNumber,
                    Amount = amount,
                    Reference = reference
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var result = httpClient.PostAsync($"{Constants.ApiBaseUrl}/withdraws", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    CliWidgets.RenderPanel($"[green]Withdrawn Q {amount:n0} to account {accountNumber} with reference {reference}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = result.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to withdraw: {(int)result.StatusCode} - {result.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                CliWidgets.RenderError($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }
        }
    }


}