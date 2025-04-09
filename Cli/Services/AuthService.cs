using Cli.Commands;
using Spectre.Console;
using System.Diagnostics;
using Cli.Models;

namespace Cli.Services
{
    public class AuthService
    {

        public static int Login()
        {
            string url = "https://example.com";
            AnsiConsole.MarkupLine($"[green]Open the following URL to login: {url}[/]");

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                User.SetUserDetails("Kong", "kong@gmail.com", "1", "jwt");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to open the URL: {ex.Message}[/]");
                return 1; // Return non-zero to indicate an error
            }
            return 0; // Return zero to indicate success
        }
    }
}