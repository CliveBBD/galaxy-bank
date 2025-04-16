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
            [CommandOption("-a|--amount <Amount>")]
            public decimal Amount { get; set; }

            [CommandOption("-f|--from <FromAccount>")]
            public string FromAccount { get; set; } = string.Empty;

            [CommandOption("-t|--to <ToAccount>")]
            public string ToAccount { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrEmpty(settings.FromAccount) || string.IsNullOrEmpty(settings.ToAccount))
            {
                AnsiConsole.MarkupLine("[red]Both from and to accounts must be specified.[/]");
                return 1;
            }

            if (settings.Amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]Amount must be greater than zero.[/]");
                return 1;
            }

            // Prompt user for FromReference and ToReference
            AnsiConsole.Markup("Enter a [green]reference[/] for the [blue]from account[/]:");
            var fromReference = ReadLine.Read();
            AnsiConsole.Markup("Enter a [green]reference[/] for the [blue]to account[/]:");
            var toReference = ReadLine.Read();

            var transferPayload = new
            {
                FromAccountNumber = settings.FromAccount,
                ToAccountNumber = settings.ToAccount,
                Amount = settings.Amount,
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
                    AnsiConsole.MarkupLine($"[green]Successfully transferred Q {settings.Amount:n0} from {settings.FromAccount} to {settings.ToAccount}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to transfer: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }
        }
    }

    public class DepositCommand : Command<DepositCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-a|--amount <Amount>")]
            public decimal Amount { get; set; }

            [CommandOption("-r|--reference <Reference>")]
            public string Reference { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrEmpty(settings.Reference))
            {
                AnsiConsole.MarkupLine("[red]Reference must be specified.[/]");
                return 1;
            }

            if (settings.Amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]Amount must be greater than zero.[/]");
                return 1;
            }

            using var httpClient = new HttpClient();

            // Add the Authorization header with the bearer token
            var bearerToken = User.Token; // Replace with the actual token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                // TODO: fetch api from configuration
                // TODO: Stop using email endpoint
                // TODO: Use account number instead
                // TODO: Use one function for both deposit and withdraw, and move transfer out
                // Fetch accounts from the API
                var response = httpClient.GetAsync($"{Constants.ApiBaseUrl}/accounts").Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                if (accounts == null || !accounts.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No accounts found.[/]");
                    return 1;
                }

                // Prepare account choices
                var accountChoices = accounts.Select(a => $"{a.AccountNumber} - {a.AccountType.Name}").ToList();

                // Prompt user to select an account
                var selectedAccount = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an [green]account[/]:")
                        .PageSize(10)
                        .AddChoices(accountChoices)
                );

                // Extract AccountId from the selected choice
                var accountNumber = selectedAccount.Split(" - ")[0];

                var payload = new
                {
                    AccountNumber = accountNumber,
                    Amount = settings.Amount,
                    Reference = settings.Reference
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                Console.WriteLine("This is the payload" + jsonPayload);
                Console.WriteLine("This is the content" + content);

                // Send the deposit/withdraw request
                var endpoint = this.GetType().Name == nameof(DepositCommand) ? "deposit" : "withdraw";
                var result = httpClient.PostAsync($"{Constants.ApiBaseUrl}/{endpoint}", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine(result.Content.ReadAsStringAsync().Result);
                    AnsiConsole.MarkupLine($"[green]{(endpoint == "deposit" ? "Deposited" : "Withdrawn")} Q {settings.Amount:n0} to account {accountNumber} with reference {settings.Reference}[/]");
                    return 0;
                }
                else
                {
                    Console.WriteLine(result.Content.ReadAsStringAsync().Result);
                    var errorMessage = result.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to {endpoint}: {result.StatusCode} - {result.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }
        }
    }


    public class WithdrawCommand : Command<WithdrawCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-a|--amount <Amount>")]
            public int Amount { get; set; }

            [CommandOption("-r|--reference <Reference>")]
            public string Reference { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (string.IsNullOrEmpty(settings.Reference))
            {
                AnsiConsole.MarkupLine("[red]Reference must be specified.[/]");
                return 1;
            }

            if (settings.Amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]Amount must be greater than zero.[/]");
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
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                    AnsiConsole.MarkupLine($"[red]Failed to fetch accounts: {response.StatusCode} - {response.ReasonPhrase}[/]");
                    return 1;
                }

                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var accounts = JsonSerializer.Deserialize<List<Account>>(jsonResponse);

                if (accounts == null || !accounts.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No accounts found.[/]");
                    return 1;
                }

                // Prepare account choices
                var accountChoices = accounts.Select(a => $"{a.AccountNumber} - {a.AccountType.Name}").ToList();

                // Prompt user to select an account
                var selectedAccount = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an [green]account[/]:")
                        .PageSize(10)
                        .AddChoices(accountChoices)
                );

                // Extract AccountId from the selected choice
                var accountNumber = selectedAccount.Split(" - ")[0];

                var payload = new
                {
                    AccountNumber = accountNumber,
                    Amount = settings.Amount,
                    Reference = settings.Reference
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the deposit/withdraw request
                var endpoint = this.GetType().Name == nameof(DepositCommand) ? "deposit" : "withdraw";
                var result = httpClient.PostAsync($"{Constants.ApiBaseUrl}/{endpoint}", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine(result.Content.ReadAsStringAsync().Result);
                    AnsiConsole.MarkupLine($"[green]{(endpoint == "deposit" ? "Deposited" : "Withdrawn")} Q {settings.Amount:n0} to account {accountNumber} with reference {settings.Reference}[/]");
                    return 0;
                }
                else
                {
                    var errorMessage = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(errorMessage);
                    AnsiConsole.MarkupLine($"[red]Failed to {endpoint}: {result.StatusCode} - {result.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
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

                        AnsiConsole.Write(table);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No transactions found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
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
                        AnsiConsole.Write(tableBuilder.Table);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No transaction types found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to fetch transaction types: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
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
            if (string.IsNullOrEmpty(settings.StartDate))
            {
                AnsiConsole.MarkupLine("[red]Start date is required.[/]");
                return 1;
            }

            if (!DateTime.TryParseExact(settings.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            {
                AnsiConsole.MarkupLine("[red]Invalid start date format. Use yyyy-MM-dd.[/]");
                return 1;
            }

            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(settings.EndDate))
            {
                if (!DateTime.TryParseExact(settings.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                {
                    AnsiConsole.MarkupLine("[red]Invalid end date format. Use yyyy-MM-dd.[/]");
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

                    Console.WriteLine("This is the response" + jsonResponse);
                    if (transactions != null && transactions.Any())
                    {
                        Console.WriteLine(transactions);
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
                            AnsiConsole.MarkupLine("[yellow]No transactions found for the specified filters.[/]");
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

                        AnsiConsole.Write(table);

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
                            AnsiConsole.MarkupLine($"[green]Statement saved to {settings.OutputFile}[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No transactions found.[/]");
                    }

                    return 0;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    AnsiConsole.MarkupLine($"[red]Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase} - {errorMessage}[/]");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
                return 1;
            }
        }
    }
}