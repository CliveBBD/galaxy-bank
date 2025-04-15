using System.Data;
using Api.Models;
using Dapper;

namespace Api.Repositories
{
    public interface ITransactionRepository
    {
        public Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId);
        public Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId);
        public Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnection _dbConnection;
        public TransactionRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId)
        {
            string query = """
            SELECT 
                t.*, 
                tt.transaction_type_id AS "TransactionTypeId", 
                tt.name AS "Name"
            FROM transactions t
            INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
            INNER JOIN accounts a ON t.account_id = a.account_id
            INNER JOIN users u ON a.user_id = u.user_id
            WHERE u.google_id = @GoogleId;
            """;

            var transactions = await _dbConnection.QueryAsync<Transaction, TransactionType, Transaction>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    return transaction;
                },
                new { GoogleId = googleId }, // Pass the googleId as a parameter
                splitOn: "TransactionTypeId"
            );

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId)
        {
            string query = """
            SELECT 
                t.*, 
                tt.transaction_type_id AS "TransactionTypeId", 
                tt.name AS "Name"
            FROM transactions t
            INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
            INNER JOIN accounts a ON t.account_id = a.account_id
            INNER JOIN users u ON a.user_id = u.user_id
            WHERE u.google_id = @GoogleId AND a.account_id = @AccountId;
            """;

            var transactions = await _dbConnection.QueryAsync<Transaction, TransactionType, Transaction>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    return transaction;
                },
                new { GoogleId = googleId, AccountId = accountId }, // Pass the googleId and accountId as parameters
                splitOn: "TransactionTypeId"
            );

            Console.WriteLine(transactions);

            return transactions;
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId)
        {
            string query = """
            SELECT 
                t.*, 
                tt.transaction_type_id AS "TransactionTypeId", 
                tt.name AS "Name"
            FROM transactions t
            INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
            INNER JOIN accounts a ON t.account_id = a.account_id
            INNER JOIN users u ON a.user_id = u.user_id
            WHERE u.google_id = @GoogleId AND t.transaction_id = @TransactionId;
            """;

            var transactions = await _dbConnection.QueryAsync<Transaction, TransactionType, Transaction>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    return transaction;
                },
                new { GoogleId = googleId, TransactionId = transactionId }, // Pass the googleId and transactionId as parameters
                splitOn: "TransactionTypeId"
            );

            return transactions;
        }
    }
}