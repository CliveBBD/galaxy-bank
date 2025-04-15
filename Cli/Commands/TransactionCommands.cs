using Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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
                FromAccountID = settings.FromAccount,
                ToAccountID = settings.ToAccount,
                Amount = settings.Amount,
                FromReference = fromReference,
                ToReference = toReference
            };

            var jsonPayload = JsonSerializer.Serialize(transferPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.PostAsync("https://localhost:7059/transfer", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully transferred Q {settings.Amount:n0} from {settings.FromAccount} to {settings.ToAccount}[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to transfer: {response.StatusCode} - {response.ReasonPhrase}[/]");
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

            // Prompt user to select an account
            var accountID = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [green]account[/] to deposit into:")
                    .PageSize(10)
                    .AddChoices(new[] { "Check", "Savings", "Credit" }) // Fake account IDs
            );

            var depositPayload = new
            {
                AccountID = 1,
                Amount = settings.Amount,
                Reference = settings.Reference
            };

            var jsonPayload = JsonSerializer.Serialize(depositPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.PostAsync("https://localhost:7059/deposit", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Deposited Q {settings.Amount:n0} to account {accountID} with reference {settings.Reference}[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to deposit: {response.StatusCode} - {response.ReasonPhrase}[/]");
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

            // Prompt user to select an account
            var accountID = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [green]account[/] to deposit into:")
                    .PageSize(10)
                    .AddChoices(new[] { "Check", "Savings", "Credit" }) // Fake account IDs
            );

            var depositPayload = new
            {
                AccountID = 1,
                Amount = settings.Amount,
                Reference = settings.Reference
            };

            var jsonPayload = JsonSerializer.Serialize(depositPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            try
            {
                var response = httpClient.PostAsync("https://localhost:7059/withdraw", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Withdrawed Q {settings.Amount:n0} from account {accountID} with reference {settings.Reference}[/]");
                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to deposit: {response.StatusCode} - {response.ReasonPhrase}[/]");
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
            public int? AccountID { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            using var httpClient = new HttpClient();
            try
            {
                string endpoint = settings.AccountID.HasValue
                    ? $"https://localhost:7059/transactions/account/{settings.AccountID}"
                    : "https://localhost:7059/transactions";

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
                                transaction.TransactionReferenceID.ToString(),
                                transaction.Reference,
                                transaction.AccountID.ToString(),
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
                    AnsiConsole.MarkupLine($"[red]Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase}[/]");
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
            try
            {
                // Replace with the actual API endpoint
                var response = httpClient.GetAsync("https://localhost:7059/transaction-types").Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var transactionTypes = JsonSerializer.Deserialize<List<TransactionType>>(jsonResponse);

                    if (transactionTypes != null && transactionTypes.Any())
                    {
                        AnsiConsole.MarkupLine("[green]Transaction Types:[/]");
                        foreach (var type in transactionTypes)
                        {
                            AnsiConsole.MarkupLine($"- [yellow]{type.TransactionTypeID}[/]: {type.Name}");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]No transaction types found.[/]");
                    }

                    return 0;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch transaction types: {response.StatusCode} - {response.ReasonPhrase}[/]");
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