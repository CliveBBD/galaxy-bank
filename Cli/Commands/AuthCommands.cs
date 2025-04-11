using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Services;

namespace Cli.Commands
{
    public class LoginCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            AuthService.Login();
            return 0;
        }
    }

    public class LogoutCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            User.Clear();
            AnsiConsole.MarkupLine($"[green]You are logged out[/]");
            return 0;
        }
    }

    public class WhoAmICommand : Command
    {
        public override int Execute(CommandContext context)
        {
            if (User.Username.Length > 0)
            {
                AnsiConsole.MarkupLine($"[green]You are logged in as {User.Username}[/]");
                AnsiConsole.MarkupLine($"[green]Email: {User.Email}[/]");
                AnsiConsole.MarkupLine($"[green]User ID: {User.Id}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]You are not logged in[/]");
            }
            return 0;
        }
    }
}