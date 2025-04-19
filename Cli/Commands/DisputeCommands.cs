using System.Text.Json;
using System.Threading.Tasks;
using Cli.Models;
using Cli.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using Api.Models;

namespace Cli.Commands
{
    public class DisputeCommand : Command<DisputeCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }
        public override int Execute(CommandContext context, Settings settings)
        {

            HttpClientWrapper http = new HttpClientWrapper(Models.User.Token);
            string requestUrl = $"{Constants.ApiBaseUrl}/transactions/disputable";
            HttpResponseMessage response = http.httpClient.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                IEnumerable<Api.Models.Transaction>? disputableTransactions = JsonSerializer.Deserialize<IEnumerable<Api.Models.Transaction>>(jsonResponse);

                if (disputableTransactions != null && disputableTransactions.Any())
                {
                    string selectedTransaction = CliWidgets.RenderSelection("Choose a transaction to dispute", disputableTransactions.Select(transaction => $"{transaction.TransactionReferenceID}: {transaction.Reference}. Payment of Q {-transaction.Amount}"));
                    string selectedTransactionReferenceId = selectedTransaction.Split(':')[0];

                    var disputeReasonsUrl = $"{Constants.ApiBaseUrl}/disputes/reasons";
                    HttpResponseMessage disputeReasonsResponse = http.httpClient.GetAsync(disputeReasonsUrl).Result;

                    if (disputeReasonsResponse.IsSuccessStatusCode)
                    {
                        string disputeReasonsJsonResponse = disputeReasonsResponse.Content.ReadAsStringAsync().Result;
                        var disputeReasons = JsonSerializer.Deserialize<IEnumerable<Api.Models.DisputeReason>>(disputeReasonsJsonResponse);
                        string reason = CliWidgets.RenderSelection("Please select a category for your dispute.", disputeReasons!.Select(reason => $"{reason.DisputeReasonID}: {reason.Description}"));
                        string disputeReasonDetails = CliWidgets.PromptText("Please provide more details about your dispute.");
                        Dictionary<string, string?> payload = new Dictionary<string, string?>
                        {
                            { "disputedTransactionReferenceID", selectedTransactionReferenceId },
                            { "details", disputeReasonDetails },
                            { "disputeReasonId", reason.Split(":")[0]}
                        };
                        requestUrl = $"{Constants.ApiBaseUrl}/disputes";
                        response = http.httpClient.PostAsync(requestUrl, JsonContent.Create(payload)).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            jsonResponse = response.Content.ReadAsStringAsync().Result;
                            Api.Models.Dispute? createdDispute = JsonSerializer.Deserialize<Api.Models.Dispute>(jsonResponse);
                            if (createdDispute != null)
                            {
                                TableBuilder<Api.Models.Dispute> tableBuilder = new TableBuilder<Api.Models.Dispute>(new List<Api.Models.Dispute>() { createdDispute });
                                CliWidgets.RenderTable("New Dispute", tableBuilder.Table);
                                return 0;
                            }
                            else
                            {
                                CliWidgets.RenderError("We had trouble creating the dispute. Please try again later.");
                                return 1;
                            }
                        }
                        else
                        {
                            CliWidgets.RenderHttpResponseAsync(response);
                            return 1;
                        }
                    }
                    else
                    {
                        CliWidgets.RenderHttpResponseAsync(disputeReasonsResponse);
                        return 1;
                    }
                }
                else
                {
                    CliWidgets.RenderWarning("There are no transactions that you may dispute.");
                    return 1;
                }
            }
            else
            {
                CliWidgets.RenderHttpResponseAsync(response);
                return 1;
            }
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
            var http = new HttpClientWrapper(Models.User.Token);
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
                        CliWidgets.RenderPaginatedTable("Disputes", tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        CliWidgets.RenderWarning("No disputes found.");
                        return 1;
                    }
                }
                else
                {
                    CliWidgets.RenderHttpResponseAsync(response);
                    return 1;
                }
            }
            catch (Exception exception)
            {
                CliWidgets.RenderError($"An error occurred: {exception.Message}");
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
                CliWidgets.RenderError("Dispute ID must be specified.");
                return 1;
            }
            else
            {
                var http = new HttpClientWrapper(Models.User.Token);
                var requestUrl = $"{Constants.ApiBaseUrl}/disputes/{settings.DisputeId}";
                var response = http.httpClient.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var dispute = JsonSerializer.Deserialize<Api.Models.Dispute>(jsonResponse);
                    if (dispute != null)
                    {
                        var tableBuilder = new TableBuilder<Api.Models.Dispute>(new List<Api.Models.Dispute>(){dispute});
                        CliWidgets.RenderTable("Dispute", tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        CliWidgets.RenderWarning("[yellow]No disputes found.[/]");
                        return 1;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    CliWidgets.RenderWarning($"[yellow]No disputes found[/]");
                    return 0;
                }
                else
                {
                    CliWidgets.RenderHttpResponseAsync(response);
                    return 1;
                }
            }
        }
    }

    public class GetDisputeHistoryByIdCommand : Command<GetDisputeHistoryByIdCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }
        public override int Execute(CommandContext context, Settings settings)
        {

            var http = new HttpClientWrapper(Models.User.Token);
            var requestUrl = $"{Constants.ApiBaseUrl}/disputes";
            var response = http.httpClient.GetAsync(requestUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var disputesForUser = JsonSerializer.Deserialize<IEnumerable<Api.Models.Dispute>>(jsonResponse);

                if (disputesForUser != null && disputesForUser.Any())
                {
                    var disputeSelection = CliWidgets.RenderSelection(
                        "Which dispute would you like to view?",
                        disputesForUser.Select(dispute => $"{dispute.DisputeID}: Disputing {dispute.DisputedTransactionReferenceID} for the reason '{dispute.Reason}'")
                    );

                    if (disputeSelection == null)
                    {
                        CliWidgets.RenderError("No dispute selected.");
                        return 1;
                    }
                    else
                    {
                        var disputeId = disputeSelection.Split(":")[0];

                        var disputeHistoryRequestUrl = $"{Constants.ApiBaseUrl}/disputes/{disputeId}/history";
                        var disputeHistoryResponse = http.httpClient.GetAsync(disputeHistoryRequestUrl).Result;

                        if (disputeHistoryResponse.IsSuccessStatusCode)
                        {

                            var disputeHistoryJsonResponse = disputeHistoryResponse.Content.ReadAsStringAsync().Result;
                            var disputeHistory = JsonSerializer.Deserialize<IEnumerable<Api.Models.DisputeHistoryEntry>>(disputeHistoryJsonResponse);
                            if (disputeHistory != null && disputeHistory.Any())
                            {
                                var tableBuilder = new TableBuilder<Api.Models.DisputeHistoryEntry>(disputeHistory);
                                CliWidgets.RenderTable("Dispute History", tableBuilder.Table);
                                return 0;
                            }
                            else
                            {
                                CliWidgets.RenderWarning("No disputes found.");
                                return 1;
                            }
                        }
                        else if (disputeHistoryResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            CliWidgets.RenderWarning($"No dispute found for the provided id {disputeId}.");
                            return 0;
                        }
                        else
                        {
                            CliWidgets.RenderHttpResponseAsync(disputeHistoryResponse);
                            return 1;
                        }
                    }
                }
                else
                {
                    CliWidgets.RenderWarning("You currently have no disputes");
                    return 1;
                }
            }
            else
            {
                CliWidgets.RenderHttpResponseAsync(response);
                return 1;
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

            var http = new HttpClientWrapper(Models.User.Token);
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
                    CliWidgets.RenderWarning("This dispute has already been resolved.");
                    return 1;
                }
            }
            else
            {
                CliWidgets.RenderHttpResponseAsync(allowedNextStatusesResponse);
                return 1;
            }

            var selectedNextStatus = CliWidgets.RenderSelection("Move to", allowedNextDisputeStatuses.Select(status => status.Name));

            var selectedStatus = allowedNextDisputeStatuses.Find(status => status.Name == selectedNextStatus);
            if (selectedStatus == null)
            {
                CliWidgets.RenderError("Selected status not found.");
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
                        CliWidgets.RenderTable("Dispute History", tableBuilder.Table);
                        return 0;
                    }
                    else
                    {
                        CliWidgets.RenderError("We had trouble updating the dispute status. Please try again later.");
                        return 1;
                    }
                }
                else
                {
                    CliWidgets.RenderHttpResponseAsync(updateStatusResponse);
                    return 1;
                }
            }
        }
    }
}