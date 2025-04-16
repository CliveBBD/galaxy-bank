using System.Text.Json;
using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Services;
using Newtonsoft.Json;

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
                    ? $"https://localhost:7059/accounts/{settings.AccountNumber}"
                    : "https://localhost:7059/accounts";

                var response = httpClient.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var accounts = System.Text.Json.JsonSerializer.Deserialize<List<Account>>(jsonResponse);

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

                        AnsiConsole.Write(table);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No accounts found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }


        }
    }
    public class ListAccountsCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            // Placeholder for account listing logic
            var accountsService = new AccountsService();
            var accounts = accountsService.GetAccountTypes().Result;
            var accountTypes = JsonConvert.DeserializeObject<List<string>>(accounts.Content.ReadAsStringAsync().Result.Trim());
            AnsiConsole.MarkupLine("[green]Listing all accounts...[/]");
            var accountType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select account type")
                    .AddChoices(accountTypes ?? []));
            Console.WriteLine(accountType);
            return 0;
        }
    }

    public class CreateAccountCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            // Placeholder for account creation logic
            AnsiConsole.MarkupLine("[green]Creating a new account...[/]");
            return 0;
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
            AnsiConsole.Write(table);

            return 0;
        }
    }


}