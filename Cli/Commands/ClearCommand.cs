using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Cli.Commands
{
public class ClearCommand : Command
{
    public override int Execute([NotNull] CommandContext context)
    {
        AnsiConsole.Clear();
        return 0;
    }
}}