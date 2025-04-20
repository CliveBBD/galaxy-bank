using Cli.Commands;

//This is just a configuration file, if it helps, consider it a json
namespace Cli.Helpers
{
    public static class CommandConfig
    {
        // Store command names and descriptions in static properties or methods
        private static readonly List<CommandInfo> commands = new List<CommandInfo>
        {
            new() { Name = "help", Description = "Show available commands", CommandType = typeof(HelpCommand) },
            new() { Name = "about", Description = "Information about Galaxy Bank", CommandType = typeof(AboutCommand) },
            new() { Name = "clear", Description = "Clear the screen", CommandType = typeof(ClearCommand) },
            new() { Name = "login", Description = "Log in to your account", CommandType = typeof(LoginCommand) },
            new() { Name = "logout", Description = "Log out of your account", CommandType = typeof(LogoutCommand) },
            new() { Name = "whoami", Description = "Show the currently logged-in user", CommandType = typeof(WhoAmICommand) },
            new() { Name = "dispute", Description = "Create a new dispute", CommandType = typeof(DisputeCommand) },
            new() { Name = "show-disputes", Description = "Show a list of disputes", CommandType = typeof(ListDisputesCommand) },
            new() { Name = "show-dispute", Description = "Retrieve a dispute by its ID", CommandType = typeof(GetDisputeByIdCommand) },
            new() { Name = "show-dispute-history", Description = "Retrieve a dispute by its ID", CommandType = typeof(GetDisputeHistoryByIdCommand) },
            new() { Name = "review-dispute", Description = "Review an existing dispute", CommandType = typeof(ResolveDisputeCommand) },
            new() { Name = "transfer", Description = "Transfer money between accounts", CommandType = typeof(TransferCommand) },
            new() { Name = "show-accounts", Description = "List all accounts", CommandType = typeof(ListAccountsCommand) },
            new() { Name = "create-account", Description = "Create a new account", CommandType = typeof(CreateAccountCommand) },
            new() {Name = "deposit", Description = "Deposit money into an account", CommandType = typeof(DepositCommand) },
            new() { Name = "withdraw", Description = "Withdraw money from an account", CommandType = typeof(WithdrawCommand) },
            new() {Name="transaction-types", Description="List all transaction types", CommandType = typeof(GetAllTransactionTypesCommand) },
            new() {Name="show-transactions", Description="Get all transactions", CommandType = typeof(GetAllTransactionsCommand) },
            new() {Name="show-statement", Description="Get account statement", CommandType = typeof(GetStatementCommand) },
        };
        public static IReadOnlyList<CommandInfo> Commands => commands;
    }

    public class CommandInfo
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Type CommandType { get; set; }
    }
}
