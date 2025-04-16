using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Services;
using Google.Apis.Auth;
using Newtonsoft.Json;
using Namotion.Reflection;

namespace Cli.Commands
{
    public class LoginCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            try
            {
                var isTokenValid = await IsTokenValid(User.Token);
                if (isTokenValid)
                {
                    Console.WriteLine("Already authenticated, proceed.");
                    return 0;
                }
                var authService = new AuthService();
                Console.WriteLine("Initiating Google authentication...");

                var result = await authService.LoginAsync();
                var payload = await GoogleJsonWebSignature.ValidateAsync(result.Token.IdToken);
                if (payload != null)
                {

                    // get user from db if exists

                    User.SetUserDetails(
                        payload.GivenName, 
                        payload.Email, 
                        payload.Subject, 
                        result.Token.IdToken,
                        result.Token.Role,
                        result.Token.SessionId
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
            catch (InvalidJwtException)
            {
                return false;
            }
        }
    }

    public class LogoutCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            var authService = new AuthService();
            var logoutResponse = await authService.LogoutAsync(User.SessionId);
            var logOut = JsonConvert.DeserializeObject(logoutResponse.Content.ReadAsStringAsync().Result);
            if(!logOut.HasProperty("Error"))
            {
                AnsiConsole.MarkupLine($"[green]You are logged out[/]");
            }
            User.Clear();
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
                AnsiConsole.MarkupLine($"[green]Role: {User.Role}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]You are not logged in[/]");
            }
            return 0;
        }
    }
}