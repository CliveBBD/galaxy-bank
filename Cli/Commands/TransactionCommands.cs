using Spectre.Console;
using Spectre.Console.Cli;

namespace Cli.Commands
{
    public class TransferCommand : Command<TransferCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-a|--amount <Amount>")]
            public decimal Amount { get; set; }

            [CommandOption("-f|--from <FromAccount>")]
            public string FromAccount { get; set; } = string.Empty;

            [CommandOption("-t|--to <ToAccount>")]
            public string ToAccount { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrEmpty(settings.FromAccount) || string.IsNullOrEmpty(settings.ToAccount))
            {
                AnsiConsole.MarkupLine("[red]Both from and to accounts must be specified.[/]");
                return 1;
            }

            if (settings.Amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]Amount must be greater than zero.[/]");
                return 1;
            }

            // Simulate transfer logic here
            AnsiConsole.MarkupLine($"[green]Transferred Q {settings.Amount:n0} from {settings.FromAccount} to {settings.ToAccount}[/]");
            return 0;
        }
    }

    public class GetAllTransactionsCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            // Placeholder for getting all transactions logic
            AnsiConsole.MarkupLine("[green]Getting all transactions...[/]");
            return 0;
        }
    }
}