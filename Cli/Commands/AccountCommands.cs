using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Services;
using Newtonsoft.Json;

namespace Cli.Commands
{
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