using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands 
{
    public class AboutCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            AnsiConsole.MarkupLine("[bold cyan]Galaxy Bank CLI v1.0[/]");
            AnsiConsole.MarkupLine("Manage your accounts, check balances, and move money in the terminal.");
            return 0;
        }
    }
}