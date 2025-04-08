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
}