using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;

namespace Cli.Shell
{
    public class Shell
    {
        public static int RunShell(CommandApp app)
        {
            var font = FigletFont.Load("fonts/ANSI_Shadow.flf");
            var banner = new FigletText(font, "Galaxy Bank")
                .Centered()
                .Color(Color.Cyan1);

            AnsiConsole.Clear();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.Write(banner);
            AnsiConsole.MarkupLine("[bold green]Welcome to the Galaxy Bank CLI Shell![/]");
            AnsiConsole.MarkupLine("[grey]Type 'help' to see available commands or 'exit' to quit.[/]\n");

            while (true)
            {
                var input = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[bold green]{User.Username}@gbank>[/] ")
                        .AllowEmpty());

                var trimmedInput = input.Trim();

                if (string.IsNullOrWhiteSpace(trimmedInput))
                {
                    continue;
                }

                if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[yellow]Exiting Galaxy Bank CLI...[/]");
                    return 0;
                }

                try
                {
                    // Split the input into arguments and run the command
                    var inputArgs = trimmedInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    app.Run(inputArgs);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                }
            }
        }
    }
}