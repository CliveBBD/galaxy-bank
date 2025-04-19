using Spectre.Console;
using Spectre.Console.Cli;
using Cli.Models;
using Cli.Services;
using Google.Apis.Auth;
using Cli.Helpers;

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
                    CliWidgets.RenderWarning("Already authenticated, proceed.");
                    return 0;
                }
                var authService = new AuthService();

                var result = await authService.LoginAsync();
                var payload = await GoogleJsonWebSignature.ValidateAsync(result.Token.IdToken);
                if (payload != null)
                {

                    // get user from db if exists

                    User.SetUserDetails(
                        payload.GivenName,
                        payload.Email,
                        payload.Subject,
                        result.Token.IdToken
                    );
                }

                if (result.Success)
                {
                    CliWidgets.RenderSuccess("Authentication successful!");
                }
                else
                {
                    CliWidgets.RenderError("Authentication failed or timed out.");
                }

                return 0;
            }
            catch (Exception ex)
            {

                CliWidgets.RenderError($"Error: {ex.Message}");
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

    public class LogoutCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            User.Clear();
            CliWidgets.RenderSuccess($"[green]You are logged out[/]");
            return 0;
        }
    }

    public class WhoAmICommand : Command
    {
        public override int Execute(CommandContext context)
        {
            if (User.Username.Length > 0)
            {
                CliWidgets.RenderPanel($"You are logged in as {User.Username}\nEmail: {User.Email}\nGoogle ID: {User.GoogleId}\nJWT: {User.Token}", "whoami");
            }
            else
            {
                CliWidgets.RenderError("You are not logged in");
            }
            return 0;
        }
    }
}
