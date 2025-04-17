using System.Text;
using System.Text.Json;
using Cli.Helpers;
using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands
{

    public class GetAccountsCommand : Command<GetAccountsCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-t|--top <Top>")]
            public int? Top { get; set; }

            [CommandOption("-i|--id <AccountNumber>")]
            public string? AccountNumber { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token);
            try
            {
                string endpoint = !string.IsNullOrWhiteSpace(settings.AccountNumber)
                    ? $"{Constants.ApiBaseUrl}/accounts/{settings.AccountNumber}"
                    : $"{Constants.ApiBaseUrl}/accounts";

                var response = httpClient.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                    if (accounts != null && accounts.Any())
                    {
                        // Apply the "Top" filter if specified
                        if (settings.Top.HasValue)
                        {
                            accounts = accounts
                                .OrderByDescending(t => t.CreatedAt)
                                .Take(settings.Top.Value)
                                .ToList();
                        }

                        // Display accounts in a table
                        var table = new Table();
                        table.AddColumn("UserId");
                        table.AddColumn("AccountType");
                        table.AddColumn("Balance");
                        table.AddColumn("Created At");
                        table.AddColumn("AccountNumber");


                        foreach (var account in accounts)
                        {
                            string formattedBalance = account.Balance < 0
                                ? $"[red]-Q {Math.Abs(account.Balance)}[/]"
                                : $"[green]Q {account.Balance}[/]";

                            table.AddRow(
                                account.UserId.ToString(),
                                account.AccountType.Name,
                                formattedBalance,
                                account.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                account.AccountNumber
                            );
                        }

                        CliWidgets.RenderTable("Accounts", table);
                    }
                    else
                    {
                        CliWidgets.RenderWarning("[yellow]No accounts found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
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
    public class ListAccountsCommand : Command<ListAccountsCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-t|--top <Top>")]
            public int? Top { get; set; }

            [CommandOption("-a|--account-number <AccountNumber>")]
            public string? AccountNumber { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token);

            try
            {
                string endpoint = !string.IsNullOrWhiteSpace(settings.AccountNumber)
                    ? $"{Constants.ApiBaseUrl}/accounts/{settings.AccountNumber}"
                    : $"{Constants.ApiBaseUrl}/accounts";

                var response = httpClient.GetAsync(endpoint).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;

                List<Account> accounts;

                if (!string.IsNullOrWhiteSpace(settings.AccountNumber))
                {
                    var account = JsonSerializer.Deserialize<Account>(jsonResponse);
                    accounts = account != null ? new List<Account> { account } : new List<Account>();
                }
                else
                {
                    accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse) ?? new List<Account>();
                }

                if (settings.Top.HasValue)
                {
                    accounts = accounts
                        .OrderByDescending(t => t.CreatedAt)
                        .Take(settings.Top.Value)
                        .ToList();
                }

                if (accounts.Any())
                {
                    DisplayAccounts(accounts);
                }
                else
                {
                    CliWidgets.RenderWarning("[yellow]No accounts found.[/]");
                }

                return 0;
            }
            catch (Exception ex)
            {
                CliWidgets.RenderError($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }

        }
        private void DisplayAccounts(List<Account> accounts)
        {
            var table = new Table();
            table.AddColumn("UserId");
            table.AddColumn("AccountType");
            table.AddColumn("Balance");
            table.AddColumn("Created At");
            table.AddColumn("AccountNumber");

            foreach (var account in accounts)
            {
                string formattedBalance = account.Balance < 0
                    ? $"[red]-Q {Math.Abs(account.Balance)}[/]"
                    : $"[green]Q {account.Balance}[/]";

                table.AddRow(
                    account.UserId.ToString(),
                    account.AccountType?.Name ?? "N/A",
                    formattedBalance,
                    account.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    account.AccountNumber
                );
            }

            AnsiConsole.Write(table);
        }

    }


    public class CreateAccountCommand : Command<CreateAccountCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }
        public override int Execute(CommandContext context, Settings settings)
        {

            var accountType = CliWidgets.RenderSelection("Select an account type", new List<string> { "checking", "savings", "credit_card" });
            if (string.IsNullOrEmpty(accountType))
            {
                CliWidgets.RenderError("[red]Account type must be specified, valid account types are 'checking', 'savings', and 'credit_card'.[/]");
                return 1;
            }

            var accountCreationPayload = new
            {
                AccountTypeName = accountType
            };

            var jsonPayload = JsonSerializer.Serialize(accountCreationPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token);

            try
            {
                var response = httpClient.PostAsync($"{Constants.ApiBaseUrl}/accounts", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    CliWidgets.RenderPanel($"[green]Successfully created a {accountType} account[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to created an account, please make sure that you are passing 'checking', 'savings', or 'credit_card' as account type[/]");
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

    public class GetAccountDetailsCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            // Placeholder for getting account details logic
            AnsiConsole.MarkupLine("[green]Getting account details...[/]");
            return 0;
        }
    }
    public class BalanceCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            // Simulate fetching balance from a service or database
            var balance = 1000; // Example balance

            // Create a table to display the balance
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[bold]Account[/]").Centered())
                .AddColumn(new TableColumn("[bold]Balance[/]").Centered());

            // Add a row with the account and balance
            table.AddRow("[green]Checking Account[/]", $"[yellow]Q {balance}[/]");

            // Render the table to the console
            CliWidgets.RenderTable("Account balance", table);

            return 0;
        }
    }


}