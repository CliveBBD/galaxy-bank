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
    public class TransferCommand : Command<TransferCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var fromAccount = CliWidgets.PromptText("From which account are transacting from");
            var toAccount = CliWidgets.PromptText("To which account are transacting to");
            if (string.IsNullOrEmpty(fromAccount) || string.IsNullOrEmpty(toAccount))
            {
                CliWidgets.RenderError("Both from and to accounts must be specified.");
                return 1;
            }

            if (!int.TryParse(CliWidgets.PromptText("Transaction amount"), out int amount) || amount <= 0)
            {
                CliWidgets.RenderError("Amount must be greater than zero.");
                return 1;
            }

            // Prompt user for FromReference and ToReference
            var fromReference = CliWidgets.PromptText("Enter a [green]reference[/] for the [blue]from account[/]:");
            var toReference = CliWidgets.PromptText("Enter a [green]reference[/] for the [blue]to account[/]:");

            var transferPayload = new
            {
                FromAccountNumber = fromAccount,
                ToAccountNumber = toAccount,
                Amount = amount,
                FromReference = fromReference,
                ToReference = toReference
            };

            var jsonPayload = JsonSerializer.Serialize(transferPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token
            try
            {
                var response = httpClient.PostAsync($"{Constants.ApiBaseUrl}/transfer", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    CliWidgets.RenderPanel($"[green]Successfully transferred Q {amount:n0} from {fromAccount} to {toAccount}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to transfer: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
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

    public class DepositCommand : Command<DepositCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var reference = CliWidgets.PromptText("Enter a reference for your transaction");
            if (string.IsNullOrEmpty(reference))
            {
                CliWidgets.RenderError("[red]Reference must be specified.[/]");
                return 1;
            }

            if (!int.TryParse(CliWidgets.PromptText("Transfer Amount"), out int amount))
            {
                CliWidgets.RenderError("Enter a valid integer");
                return 1;
            }
            if (amount <= 0)
            {
                CliWidgets.RenderError("[red]Amount must be greater than zero.[/]");
                return 1;
            }

            using var httpClient = new HttpClient();

            // Add the Authorization header with the bearer token
            var bearerToken = User.Token; // Replace with the actual token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {

                var response = httpClient.GetAsync($"{Constants.ApiBaseUrl}/accounts").Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                if (accounts == null || !accounts.Any())
                {
                    CliWidgets.RenderWarning("[yellow]No accounts found.[/]");
                    return 1;
                }

                // Prepare account choices
                var accountChoices = accounts.Select(a => $"{a.AccountNumber} - {a.AccountType.Name}").ToList();

                // Prompt user to select an account
                var selectedAccount = CliWidgets.RenderSelection("Select an account", accountChoices);

                // Extract AccountId from the selected choice
                var accountNumber = selectedAccount.Split(" - ")[0];

                var payload = new
                {
                    AccountNumber = accountNumber,
                    Amount = amount,
                    Reference = reference
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the deposit/withdraw request
                var endpoint = this.GetType().Name == nameof(DepositCommand) ? "deposit" : "withdraw";
                var result = httpClient.PostAsync($"{Constants.ApiBaseUrl}/{endpoint}", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    CliWidgets.RenderPanel($"[green]{(endpoint == "deposit" ? "Deposited" : "Withdrawn")} Q {amount:n0} to account {accountNumber} with reference {reference}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = result.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to {endpoint}: {result.StatusCode} - {result.ReasonPhrase} - {errorMessage}[/]");
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


    public class WithdrawCommand : Command<WithdrawCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var reference = CliWidgets.PromptText("Enter a reference for your transaction");
            if (string.IsNullOrEmpty(reference))
            {
                CliWidgets.RenderError("[red]Reference must be specified.[/]");
                return 1;
            }

            if (!int.TryParse(CliWidgets.PromptText("Transfer Amount"), out int amount))
            {
                CliWidgets.RenderError("Enter a valid integer");
                return 1;
            }
            if (amount <= 0)
            {
                CliWidgets.RenderError("[red]Amount must be greater than zero.[/]");
                return 1;
            }
            using var httpClient = new HttpClient();

            // Add the Authorization header with the bearer token
            var bearerToken = User.Token; // Replace with the actual token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                // Fetch accounts from the API
                // TODO: fetch api from configuration
                // TODO: Stop using email endpoint
                // TODO: api endpoints should start with /api
                // CUSTOM: Use account number instead
                var response = httpClient.GetAsync($"{Constants.ApiBaseUrl}/accounts").Result;

                if (!response.IsSuccessStatusCode)
                {
                    CliWidgets.RenderError($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                if (accounts == null || !accounts.Any())
                {
                    CliWidgets.RenderWarning("[yellow]No accounts found.[/]");
                    return 1;
                }

                // Prepare account choices
                var accountChoices = accounts.Select(a => $"{a.AccountNumber} - {a.AccountType.Name}").ToList();

                // Prompt user to select an account
                var selectedAccount = CliWidgets.RenderSelection("Select an account", accountChoices);

                // Extract AccountId from the selected choice
                var accountNumber = selectedAccount.Split(" - ")[0];

                var payload = new
                {
                    AccountNumber = accountNumber,
                    Amount = amount,
                    Reference = reference
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the deposit/withdraw request
                var endpoint = this.GetType().Name == nameof(DepositCommand) ? "deposit" : "withdraw";
                var result = httpClient.PostAsync($"{Constants.ApiBaseUrl}/{endpoint}", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    CliWidgets.RenderPanel($"[green]{(endpoint == "deposit" ? "Deposited" : "Withdrawn")} Q {amount:n0} to account {accountNumber} with reference {reference}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = result.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to {endpoint}: {result.StatusCode} - {result.ReasonPhrase} - {errorMessage}[/]");
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
                                transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
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
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    CliWidgets.RenderError($"[red]Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
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

    public class GetAllTransactionTypesCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.Token); // Replace with the actual token
            try
            {
                // Replace with the actual API endpoint
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
            [CommandOption("-s|--start <YYYY-MM-DD>")]
            public string StartDate { get; set; } = string.Empty;

            [CommandOption("-e|--end <YYYY-MM-DD>")]
            public string? EndDate { get; set; }

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
                                transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                            );
                        }

                        CliWidgets.RenderPaginatedTable("Transactions", table);

                        // Save to PDF if output file is specified
                        if (!string.IsNullOrEmpty(settings.OutputFile))
                        {
                            var pdfDocument = new PdfDocument();
                            var page = pdfDocument.AddPage();
                            var graphics = XGraphics.FromPdfPage(page);
                            var font = new XFont("Arial", 12);

                            graphics.DrawString("Transaction Statement", font, XBrushes.Black, new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);

                            int yOffset = 50;
                            foreach (var transaction in transactions)
                            {
                                var line = $"ID: {transaction.TransactionID}, Ref: {transaction.Reference}, Account: {transaction.AccountNumber}, Amount: {transaction.Amount}, Type: {transaction.TransactionType.Name}, Balance: {transaction.BalanceAfterTransaction}, Date: {transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}";
                                graphics.DrawString(line, font, XBrushes.Black, new XRect(20, yOffset, page.Width - 40, 20), XStringFormats.TopLeft);
                                yOffset += 20;

                                if (yOffset > page.Height - 50)
                                {
                                    page = pdfDocument.AddPage();
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
                    CliWidgets.RenderError($"[red]Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
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
}