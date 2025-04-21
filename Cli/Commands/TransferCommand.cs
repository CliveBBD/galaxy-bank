using Cli.Helpers;
using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text;
using System.Text.Json;

namespace Cli.Commands
{
    public class TransferCommand : Command
    {

        public override int Execute(CommandContext context)
        {

            if (!int.TryParse(CliWidgets.PromptText("Transaction amount"), out int amount) || amount <= 0)
            {
                CliWidgets.RenderError("Amount must be greater than zero.\nNote that amounts greater than Q 2 147 483 647 can't be transferred via the Cli, please visit the nearest galaxy bank.");
                return 1;
            }

            var http = new HttpClientWrapper(Models.User.Token);
            var requestUrl = $"{Constants.ApiBaseUrl}/accounts";
            var response = http.httpClient.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accountsForUser = JsonSerializer.Deserialize<IEnumerable<Api.DTOs.AccountResponse>>(jsonResponse);

                if (accountsForUser != null && accountsForUser.Any())
                {
                    var selectedAccount = CliWidgets.RenderSelection(
                        "Please select an account to transfer from",
                        accountsForUser.Select(account => $"[green]{account.AccountNumber}: ({account.AccountType.Name}) Balance Q {account.Balance}[/]")
                    );

                    var fromAccount = selectedAccount!.Split(":")[0].Split("]")[1];
                    var toAccount = CliWidgets.PromptText("To which account are you transacting to");
                    if (string.IsNullOrEmpty(fromAccount) || string.IsNullOrEmpty(toAccount))
                    {
                        CliWidgets.RenderError("Both from and to accounts must be specified.");
                        return 1;
                    }

                    // Prompt user for FromReference and ToReference
                    var fromReference = CliWidgets.PromptText("Enter a [green]reference[/] for the [blue]from account[/]:");
                    var toReference = CliWidgets.PromptText("Enter a [green]reference[/] for the [blue]to account[/]:");

                    var transferPayload = new
                    {
                        FromAccountNumber = fromAccount,
                        ToAccountNumber = toAccount,
                        Amount = amount,
                        FromReference = fromReference,
                        ToReference = toReference
                    };

                    var jsonPayload = JsonSerializer.Serialize(transferPayload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token
                    try
                    {
                        response = httpClient.PostAsync($"{Constants.ApiBaseUrl}/transfers", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            CliWidgets.RenderPanel($"[green]Successfully transferred Q {amount:n0} from {fromAccount} to {toAccount}[/]");
                            return 0;
                        }
                        else
                        {
                            CliWidgets.RenderHttpResponseAsync(response);
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        CliWidgets.RenderError($"[red]An error occurred: {ex.Message}[/]");
                        return 1;
                    }
                }
                else
                {
                    CliWidgets.RenderWarning("You do not have any accounts.");
                    return 1;
                }
            }
            else
            {
                CliWidgets.RenderHttpResponseAsync(response);
                return 1;
            }
        }
    }

}