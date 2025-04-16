using System.Text.Json;
using System.Threading.Tasks;
using Cli.Models;
using Cli.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace Cli.Commands
{
    public class DisputeCommand : Command<DisputeCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-t|--transaction-reference <TransactionReferenceId>")]
            public int TransactionReferenceId { get; set; }

            [CommandOption("-r|--reason <Reason>")]
            public string Reason { get; set; } = string.Empty;
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.TransactionReferenceId == 0 || string.IsNullOrEmpty(settings.Reason))
            {
                AnsiConsole.MarkupLine("[red]Transaction ID and reason must be specified.[/]");
                return 1;
            }

            // Simulate dispute logic here
            AnsiConsole.MarkupLine($"[green]Disputed transaction {settings.TransactionReferenceId} for reason: {settings.Reason}, waiting for approval[/]");
            return 0;
        }
    }

    public class ListDisputesCommand : Command<ListDisputesCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-u|--user-id <UserId>")]
            public int? UserId { get; set; }
            [CommandOption("-e|--email <Email>")]
            public string? Email { get; set; } = string.Empty;
            [CommandOption("-s|--status <Status>")]
            public string? Status { get; set; } = string.Empty;
            [CommandOption("-l|--limit <Limit>")]
            public int? Limit { get; set; }
            [CommandOption("-o|--offset <Offset>")]
            public int? Offset { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            var http = new HttpClientWrapper(User.Token);
            try
            {
                var queryParameters = new Dictionary<string, string?>
                {
                    { "userId", settings.UserId?.ToString() },
                    { "email", settings.Email },
                    { "status", settings.Status },
                    { "limit", settings.Limit?.ToString() },
                    { "offset", settings.Offset?.ToString() }
                };
                var requestUrl = QueryHelpers.AddQueryString($"{Constants.ApiBaseUrl}/disputes", queryParameters);
                var response = http.httpClient.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var disputes = JsonSerializer.Deserialize<List<Api.Models.Dispute>>(jsonResponse);
                    if (disputes != null && disputes.Any())
                    {
                        var tableBuilder = new TableBuilder<Api.Models.Dispute>(disputes);
                        AnsiConsole.Write(tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No disputes found.[/]");
                        return 1;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch disputes: {response.ReasonPhrase}[/]");
                    return 1;
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {exception.Message}[/]");
                return 1;
            }
        }
    }

    public class GetDisputeByIdCommand : Command<GetDisputeByIdCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-i|--dispute-id <DisputeId>")]
            public int DisputeId { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.DisputeId == 0)
            {
                AnsiConsole.MarkupLine("[red]Dispute ID must be specified.[/]");
                return 1;
            }
            else
            {
                var http = new HttpClientWrapper(User.Token);
                var requestUrl = $"{Constants.ApiBaseUrl}/disputes/{settings.DisputeId}";
                var response = http.httpClient.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var dispute = JsonSerializer.Deserialize<Api.Models.Dispute>(jsonResponse);
                    if (dispute != null)
                    {
                        var tableBuilder = new TableBuilder<Api.Models.Dispute>(new List<Api.Models.Dispute>(){dispute});
                        AnsiConsole.Write(tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No disputes found.[/]");
                        return 1;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]No disputes found[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch disputes: {response.ReasonPhrase}[/]");
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(responseBody);
                    return 1;
                }
            }
        }
    }

    public class GetDisputeHistoryByIdCommand : Command<GetDisputeHistoryByIdCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-i|--dispute-id <DisputeId>")]
            public int DisputeId { get; set; }
            [CommandOption("-l|--limit <Limit>")]
            public int? Limit { get; set; }
            [CommandOption("-o|--offset <Offset>")]
            public int? Offset { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.DisputeId == 0)
            {
                AnsiConsole.MarkupLine("[red]Dispute ID must be specified.[/]");
                return 1;
            }
            else
            {
                var queryParameters = new Dictionary<string, string?>
                {
                    { "limit", settings.Limit?.ToString() },
                    { "offset", settings.Offset?.ToString() }
                };
                var http = new HttpClientWrapper(User.Token);
                var requestUrl = QueryHelpers.AddQueryString($"{Constants.ApiBaseUrl}/disputes/{settings.DisputeId}/history", queryParameters);
                var response = http.httpClient.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var disputeHistory = JsonSerializer.Deserialize<List<Api.Models.DisputeHistoryEntry>>(jsonResponse);
                    if (disputeHistory != null && disputeHistory.Any())
                    {
                        var tableBuilder = new TableBuilder<Api.Models.DisputeHistoryEntry>(disputeHistory);
                        AnsiConsole.Write(tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No disputes found.[/]");
                        return 1;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]No dispute found for the provided id {settings.DisputeId}.[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch disputes: {response.ReasonPhrase}[/]");
                    return 1;
                }
            }
        }
    }

    public class ResolveDisputeCommand : Command<ResolveDisputeCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-i|--dispute-id <DisputeId>")]
            public int DisputeId { get; set; }
        }
        public override int Execute(CommandContext context, Settings settings)
        {

            var http = new HttpClientWrapper(User.Token);
            var requestUrl = $"{Constants.ApiBaseUrl}/disputes/{settings.DisputeId}/allowed-next-statuses";
            var allowedNextStatusesResponse = http.httpClient.GetAsync(requestUrl).Result;

            List<Api.Models.DisputeStatus>? allowedNextDisputeStatuses;
            if (allowedNextStatusesResponse.IsSuccessStatusCode)
            {
                
                var jsonResponse = allowedNextStatusesResponse.Content.ReadAsStringAsync().Result;
                allowedNextDisputeStatuses = JsonSerializer.Deserialize<List<Api.Models.DisputeStatus>>(jsonResponse);
                if (allowedNextDisputeStatuses != null && allowedNextDisputeStatuses.Any())
                {
                    // continue with the rest of this function
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]This dispute has already been resolved.[/]");
                    return 1;
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Failed to fetch disputes: {allowedNextStatusesResponse.ReasonPhrase}[/]");
                return 1;
            }

            var selectedNextStatus = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Move to")
                    .AddChoices(allowedNextDisputeStatuses.Select(status => status.Name)));

            var selectedStatus = allowedNextDisputeStatuses.Find(status => status.Name == selectedNextStatus);
            if (selectedStatus == null)
            {
                AnsiConsole.MarkupLine("[red]Selected status not found.[/]");
                return 1;
            }
            else
            {
                var payload = new Dictionary<string, string?>
                {
                    { "newStatusId", selectedStatus.DisputeStatusID.ToString() },
                };
                requestUrl = $"{Constants.ApiBaseUrl}/disputes/{settings.DisputeId}/status";
                var updateStatusResponse = http.httpClient.PostAsync(requestUrl, JsonContent.Create(payload)).Result;

                if (updateStatusResponse.IsSuccessStatusCode)
                {
                    var jsonResponse = updateStatusResponse.Content.ReadAsStringAsync().Result;
                    var insertedDisputeHistory = JsonSerializer.Deserialize<Api.Models.DisputeHistoryEntry>(jsonResponse);
                    if (insertedDisputeHistory != null)
                    {
                        var tableBuilder = new TableBuilder<Api.Models.DisputeHistoryEntry>(new List<Api.Models.DisputeHistoryEntry>(){insertedDisputeHistory});
                        AnsiConsole.Write(tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]We had trouble updating the dispute status. Please try again later.[/]");
                        return 1;
                    }
                }
                else
                {
                    var content = updateStatusResponse.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(content);
                    AnsiConsole.MarkupLine($"[red]Failed to update the status of dispute {settings.DisputeId}: {updateStatusResponse.ReasonPhrase}[/]");
                    return 1;
                }
            }
        }
    }
}