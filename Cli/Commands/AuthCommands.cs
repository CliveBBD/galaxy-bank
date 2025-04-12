using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Services;
using System.Threading.Tasks;


namespace Cli.Commands
{
    public class LoginCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
           try
            {
                var tokenManager = new TokenManager();
                var apiClient = new AuthService(tokenManager);
    
                Console.WriteLine("Initiating Google authentication...");
    
                var result = await apiClient.LoginAsync();
    
                if (result.Success)
                {
                    await tokenManager.SaveTokenAsync(result.Token);
                    Console.WriteLine("Authentication successful!");
                }
                else
                {
                    Console.WriteLine("Authentication failed or timed out.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
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