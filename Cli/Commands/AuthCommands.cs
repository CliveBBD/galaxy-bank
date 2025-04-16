using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Services;
using Google.Apis.Auth;

namespace Cli.Commands
{
    public class LoginCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
           try
            {
                var isTokenValid = await IsTokenValid(User.Token);
                if(isTokenValid) 
                {
                    Console.WriteLine("Already authenticated, proceed.");
                    return 0;
                }
                var authService = new AuthService();
                Console.WriteLine("Initiating Google authentication...");
    
                var result = await authService.LoginAsync();
                var payload = await GoogleJsonWebSignature.ValidateAsync(result.Token.IdToken);
                if(payload != null)
                {
                    User.SetUserDetails(
                        payload.GivenName, 
                        payload.Email, 
                        payload.Subject, 
                        result.Token.IdToken
                    );
                }
    
                if (result.Success)
                {
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

        public static async Task<bool> IsTokenValid(string jwt)
        {
            try 
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(jwt);
                return true; 
            }
            catch(InvalidJwtException)
            {
                return false;
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
                AnsiConsole.MarkupLine($"[green]Google ID: {User.GoogleId}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]You are not logged in[/]");
            }
            return 0;
        }
    }
}