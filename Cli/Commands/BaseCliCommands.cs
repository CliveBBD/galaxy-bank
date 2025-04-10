using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands
{
    public class HelpCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Command[/]");
            table.AddColumn("[bold]Description[/]");

            table.AddRow("help", "Show available commands");
            table.AddRow("about", "Information about Galaxy Bank");
            table.AddRow("clear", "Clear the screen");
            table.AddRow("exit", "Exit the shell");
            table.AddRow("print <message>", "Print a custom message");
            table.AddRow("login", "Log in to your account");
            table.AddRow("logout", "Log out of your account");
            table.AddRow("whoami", "Show the currently logged-in user");
            table.AddRow("dispute", "Create a new dispute");
            table.AddRow("get-dispute-by-id", "Retrieve a dispute by its ID");
            table.AddRow("resolve-dispute", "Resolve an existing dispute");
            table.AddRow("transfer", "Transfer money between accounts");
            table.AddRow("show-balance", "Display the current account balance");
            table.AddRow("show-accounts", "List all accounts");
            table.AddRow("create-account", "Create a new account");
            table.AddRow("get-account-details", "Get details of a specific account");

            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class AboutCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            AnsiConsole.MarkupLine("[bold cyan]Galaxy Bank CLI v1.0[/]");
            AnsiConsole.MarkupLine("Manage your accounts, check balances, and move money in the terminal.");
            return 0;
        }
    }

    public class ClearCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            AnsiConsole.Clear();
            return 0;
        }
    }
}