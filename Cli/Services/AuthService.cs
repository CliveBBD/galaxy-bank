using Cli.Commands;
using Spectre.Console;
using System.Diagnostics;
using Cli.Models;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Services
{
    public class AuthService
    {

        public static int Login()
        {
            StartListener();
            string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/userinfo.profile&response_type=code&redirect_uri=http://localhost:8080/oauth2callback&client_id=438794123703-9aqbuhmv0asuhr074hqd5o2lf7c7rpap.apps.googleusercontent.com";
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

        public static HttpListener  StartListener() 
        {
            try
            {
                int port = 8080;
                string prefix = $"http://localhost:{port}/"; 

                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(prefix);

                listener.Start();
                return listener;
            }
            catch(Exception e)
            {
                new HttpListener();
            }
            return new HttpListener();
        }
    }
}