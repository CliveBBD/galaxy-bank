using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Helpers;

namespace Cli.Commands
{
    public class HelpCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Command[/]");
            table.AddColumn("[bold]Description[/]");

            foreach (var command in CommandConfig.Commands)
            {
                table.AddRow($"[green]{command.Name}[/]", command.Description);
            }
            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class AboutCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            CliWidgets.RenderText("[bold cyan]Galaxy Bank CLI v1.0[/]");
            CliWidgets.RenderText("Manage your accounts, check balances, and move money in the terminal.");
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