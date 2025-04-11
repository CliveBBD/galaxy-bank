﻿using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using Cli.Commands;
using Cli.Shell;
using Cli.Services;

// Command to print a custom message
public class PrintCommand : Command<PrintCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<message>")]
        public string Message { get; set; } = string.Empty;
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        AnsiConsole.MarkupLine($"[cyan]You entered:[/] {settings.Message}");
        return 0;
    }
}

// Application entry point

class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        // Register commands
        app.Configure(config =>
        {
            config.AddCommand<HelpCommand>("help");
            config.AddCommand<AboutCommand>("about");
            config.AddCommand<ClearCommand>("clear");

            config.AddCommand<PrintCommand>("print");

            config.AddCommand<LoginCommand>("login");
            config.AddCommand<LogoutCommand>("logout");
            config.AddCommand<WhoAmICommand>("whoami");

            config.AddCommand<DisputeCommand>("dispute");
            config.AddCommand<GetDisputeByIdCommand>("get-dispute-by-id");
            config.AddCommand<ResolveDisputeCommand>("resolve-dispute");

            config.AddCommand<TransferCommand>("transfer");
            config.AddCommand<BalanceCommand>("show-balance");
            config.AddCommand<ListAccountsCommand>("show-accounts");
            config.AddCommand<ListDisputesCommand>("create-account");
            config.AddCommand<GetAccountDetailsCommand>("get-account-details");

        });

        // If arguments are provided, run the command directly
        if (args.Length > 0)
        {
            return app.Run(args);
        }

        // Otherwise, start the interactive shell
        return Shell.RunShell(app);
    }
}
