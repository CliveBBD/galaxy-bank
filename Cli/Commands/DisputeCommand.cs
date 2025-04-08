using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands
{
    public class DisputeCommand : Command<DisputeCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-t|--transaction <TransactionId>")]
            public string TransactionId { get; set; } = string.Empty;

            [CommandOption("-r|--reason <Reason>")]
            public string Reason { get; set; } = string.Empty;
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrEmpty(settings.TransactionId) || string.IsNullOrEmpty(settings.Reason))
            {
                AnsiConsole.MarkupLine("[red]Transaction ID and reason must be specified.[/]");
                return 1;
            }

            // Simulate dispute logic here
            AnsiConsole.MarkupLine($"[green]Disputed transaction {settings.TransactionId} for reason: {settings.Reason}, waiting for approval[/]");
            return 0;
        }
    }
}