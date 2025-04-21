using Cli.Helpers;
using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Globalization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Cli.Commands
{
    public class GetAllTransactionsCommand : Command<GetAllTransactionsCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-t|--top <Top>")]
            public int? Top { get; set; }

            [CommandOption("-i|--id <AccountID>")]
            public string? AccountNumber { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token
            try
            {
                string endpoint = !string.IsNullOrEmpty(settings.AccountNumber)
                    ? $"{Constants.ApiBaseUrl}/transactions/account/{settings.AccountNumber}"
                    : $"{Constants.ApiBaseUrl}/transactions";

                var response = httpClient.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var transactions = JsonSerializer.Deserialize<List<Transaction>>(jsonResponse);

                    if (transactions != null && transactions.Any())
                    {
                        // Apply the "Top" filter if specified
                        if (settings.Top.HasValue)
                        {
                            transactions = transactions
                                .OrderByDescending(t => t.CreatedAt)
                                .Take(settings.Top.Value)
                                .ToList();
                        }

                        // Display transactions in a table
                        var table = new Table();
                        table.AddColumn("Transaction ID");
                        table.AddColumn("Transaction Reference ID");
                        table.AddColumn("Reference");
                        table.AddColumn("Account Number");
                        table.AddColumn("Amount");
                        table.AddColumn("Type");
                        table.AddColumn("Balance After");
                        table.AddColumn("Created At");

                        foreach (var transaction in transactions)
                        {
                            string formattedAmount = transaction.Amount < 0
                                ? $"[red]-Q {Math.Abs(transaction.Amount)}[/]"
                                : $"[green]Q {transaction.Amount}[/]";

                            string formattedBalance = transaction.BalanceAfterTransaction < 0
                                ? $"[red]-Q {Math.Abs(transaction.BalanceAfterTransaction)}[/]"
                                : $"[green]Q {transaction.BalanceAfterTransaction}[/]";

                            table.AddRow(
                                transaction.TransactionID.ToString(),
                                transaction.TransactionReferenceID.ToString(),
                                transaction.Reference,
                                transaction.AccountNumber.ToString(),
                                formattedAmount,
                                transaction.TransactionType.Name,
                                formattedBalance,
                                transaction.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                            );
                        }

                        CliWidgets.RenderPaginatedTable("Transaction History", table);
                    }
                    else
                    {
                        CliWidgets.RenderWarning("[yellow]No transactions found.[/]");
                    }

                    return 0;
                }
                else
                {
                    CliWidgets.RenderHttpResponseAsync(response);
                    return 1;
                }
            }
            catch (Exception exception)
            {
                CliWidgets.RenderError(exception);
                return 1;
            }
        }
    }

    public class GetAllTransactionTypesCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token
            try
            {
                var response = httpClient.GetAsync($"{Constants.ApiBaseUrl}/transaction-types").Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var transactionTypes = JsonSerializer.Deserialize<List<TransactionType>>(jsonResponse);

                    if (transactionTypes != null && transactionTypes.Any())
                    {
                        var tableBuilder = new TableBuilder<TransactionType>(transactionTypes);
                        CliWidgets.RenderTable("Transaction Types", tableBuilder.Table);
                    }
                    else
                    {
                        CliWidgets.RenderWarning("[yellow]No transaction types found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch transaction types: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                CliWidgets.RenderError($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }
        }
    }

    public class GetStatementCommand : Command<GetStatementCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-o|--output <OutputFile>")]
            public string? OutputFile { get; set; }

            [CommandOption("-n|--id <AccountNumber>")]
            public string? AccountNumber { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var startDateString = CliWidgets.PromptText("Statement start date in the format YYYY-MM-DD");
            if (string.IsNullOrEmpty(startDateString))
            {
                CliWidgets.RenderError("[red]Start date is required.[/]");
                return 1;
            }

            if (!DateTime.TryParseExact(startDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            {
                CliWidgets.RenderError("[red]Invalid start date format. Use yyyy-MM-dd.[/]");
                return 1;
            }

            DateTime? endDate = null;
            var endDateString = CliWidgets.PromptText("Statement end date in the format YYYY-MM-DD");

            if (!string.IsNullOrEmpty(endDateString))
            {
                if (!DateTime.TryParseExact(endDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                {
                    CliWidgets.RenderError("[red]Invalid end date format. Use yyyy-MM-dd.[/]");
                    return 1;
                }
                endDate = parsedEndDate;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token

            try
            {
                var endpoint = $"{Constants.ApiBaseUrl}/transactions";
                var response = httpClient.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var transactions = JsonSerializer.Deserialize<List<Transaction>>(jsonResponse);

                    if (transactions != null && transactions.Any())
                    {
                        // Filter transactions by date range
                        transactions = transactions
                            .Where(t => t.CreatedAt >= startDate && (!endDate.HasValue || t.CreatedAt <= endDate.Value))
                            .ToList();

                        // Filter by AccountID if provided
                        if (!string.IsNullOrEmpty(settings.AccountNumber))
                        {
                            transactions = transactions
                                .Where(t => t.AccountNumber == settings.AccountNumber)
                                .ToList();
                        }

                        if (!transactions.Any())
                        {
                            CliWidgets.RenderWarning("[yellow]No transactions found for the specified filters.[/]");
                            return 0;
                        }

                        // Display transactions in a table
                        var table = new Table();
                        table.AddColumn("Transaction ID");
                        table.AddColumn("Reference");
                        table.AddColumn("Account ID");
                        table.AddColumn("Amount");
                        table.AddColumn("Type");
                        table.AddColumn("Balance After");
                        table.AddColumn("Created At");

                        foreach (var transaction in transactions)
                        {
                            string formattedAmount = transaction.Amount < 0
                                ? $"[red]-Q {Math.Abs(transaction.Amount)}[/]"
                                : $"[green]Q {transaction.Amount}[/]";

                            string formattedBalance = transaction.BalanceAfterTransaction < 0
                                ? $"[red]-Q {Math.Abs(transaction.BalanceAfterTransaction)}[/]"
                                : $"[green]Q {transaction.BalanceAfterTransaction}[/]";

                            table.AddRow(
                                transaction.TransactionID.ToString(),
                                transaction.Reference,
                                transaction.AccountNumber.ToString(),
                                formattedAmount,
                                transaction.TransactionType.Name,
                                formattedBalance,
                                transaction.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                            );
                        }

                        CliWidgets.RenderPaginatedTable("Transactions", table);

                        // Save to PDF if output file is specified
                        if (!string.IsNullOrEmpty(settings.OutputFile))
                        {
                            var pdfDocument = new PdfDocument();
                            var page = pdfDocument.AddPage();
                            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                            var graphics = XGraphics.FromPdfPage(page);
                            var font = new XFont("Arial", 12);

                            graphics.DrawString("Transaction Statement", font, XBrushes.Black, new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);

                            int yOffset = 50;
                            foreach (var transaction in transactions)
                            {
                                var line = $"ID: {transaction.TransactionID}, Ref: {transaction.Reference}, Account: {transaction.AccountNumber}, Amount: {transaction.Amount}, Type: {transaction.TransactionType.Name}, Balance: {transaction.BalanceAfterTransaction}, Date: {transaction.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
                                graphics.DrawString(line, font, XBrushes.Black, new XRect(20, yOffset, page.Width - 40, 20), XStringFormats.TopLeft);
                                yOffset += 20;

                                if (yOffset > page.Height - 50)
                                {
                                    page = pdfDocument.AddPage();
                                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                    graphics = XGraphics.FromPdfPage(page);
                                    yOffset = 50;
                                }
                            }

                            pdfDocument.Save(settings.OutputFile);
                            CliWidgets.RenderPanel($"[green]Statement saved to {settings.OutputFile}[/]");
                        }
                    }
                    else
                    {
                        CliWidgets.RenderWarning("[yellow]No transactions found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderHttpResponseAsync(response);
                    return 1;
                }
            }
            catch (Exception exception)
            {
                CliWidgets.RenderError(exception);
                return 1;
            }
        }
    }
}