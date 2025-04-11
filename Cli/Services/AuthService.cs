using Spectre.Console;
using System.Net;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Diagnostics;
using Cli.Models;

namespace Cli.Services
{
    public class AuthService
    {

        public static async Task<int> Login()
        {
            string url = "https://accounts.google.com/o/oauth2/auth/client_id=438794123703-9aqbuhmv0asuhr074hqd5o2lf7c7rpap.apps.googleusercontent.com&redirect_uri=https://localhost:7059/signin-google&response_type=code&scope=openid%20email%20profile";
            AnsiConsole.MarkupLine($"[green]Open the following URL to login: {url}[/]");

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                // User.SetUserDetails("Kong", "kong@gmail.com", "1", "jwt");
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
                Console.WriteLine("Starting up local listener.....");
                int port = 8080;
                string prefix = $"http://localhost:{port}/signin-google"; 

                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                return listener;
            }
            catch(Exception)
            {
                // run default logic in here
            }
            return new HttpListener();
        }

        static async Task HandleRequestsAsync(HttpListener listener)
        {
            while (listener.IsListening)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
        }

        static async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                string responseString = "<html><body>Hello, World!</body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
    }
}