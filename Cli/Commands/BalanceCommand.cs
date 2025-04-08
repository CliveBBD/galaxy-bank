using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands
{
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