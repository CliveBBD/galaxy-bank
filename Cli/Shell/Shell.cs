using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Helpers;
using System.Text.RegularExpressions;

namespace Cli.Shell
{
    public class Shell
    {

        public static void DisplayBanner()
        {
            var font = FigletFont.Load("Fonts/ANSI_Shadow.flf");
            var banner = new FigletText(font, "Galaxy Bank")
                .Centered()
                .Color(Color.Cyan1);

            AnsiConsole.Clear();
            AnsiConsole.Write(banner);
            AnsiConsole.MarkupLine("[bold green]Welcome to the Galaxy Bank CLI Shell![/]");
            AnsiConsole.MarkupLine("[grey]Type 'help' to see available commands or 'exit' to quit.[/]\n");
        }

        public static int RunShell(CommandApp app)
        {
            DisplayBanner();

            ReadLine.AutoCompletionHandler = new CommandAutoComplete(app);

            while (true)
            {

                AnsiConsole.Markup($"[bold green]{User.Username}@gbank> [/]");
                var input = ReadLine.Read();

                var trimmedInput = input.Trim();

                ReadLine.AddHistory(trimmedInput);

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
                    // Use Regex to split input while preserving quoted strings
                    var inputArgs = Regex.Matches(trimmedInput, @"[\""].+?[\""]|[^ ]+")
                                         .Select(m => m.Value.Trim('"'))
                                         .ToArray();
                    app.Run(inputArgs);

                    if (trimmedInput.Equals("clear", StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayBanner();
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                }
            }
        }
    }
    // Custom auto-completion handler
    public class CommandAutoComplete : IAutoCompleteHandler
    {
        private readonly List<string> _commands;

        public CommandAutoComplete(CommandApp app)
        {
            // Extract command names (strings) from CommandInfo and store them in _commands list
            _commands = CommandConfig.Commands.Select(command => command.Name).ToList();
        }

        public char[] Separators { get; set; } = [' '];

        public string[] GetSuggestions(string text, int index)
        {
            return string.IsNullOrEmpty(text)
                ? [.. _commands]
                : _commands.Where(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }

}