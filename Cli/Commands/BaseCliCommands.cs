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