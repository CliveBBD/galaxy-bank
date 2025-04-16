using System.Data;
using Api.Models;
using Api.Shared;
using Api.DTOs;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface ITransactionRepository
    {
        public Task<IEnumerable<TransactionRequest>> GetTransactionsAsync(string googleId);
        public Task<IEnumerable<TransactionRequest>> GetTransactionsByAccountNumberAsync(string accountNumber, string googleId);
        public Task<IEnumerable<TransactionRequest>> GetTransactionsByIdAsync(int transactionId, string googleId);
        public Task<IEnumerable<TransactionRequest>> GetTransactionsByTransactionReferenceIdAsync(int transactionReferenceId, NpgsqlTransaction? transaction = null);
        public Task<bool> InsertReversalTransactions(IEnumerable<int> transactionIdsToReverse, NpgsqlTransaction? transaction = null);
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
                t.transaction_id,
                t.transaction_reference_id,
                t.reference,
                t.amount,
                a.account_number AS "AccountNumber",
                t.balance_after_transaction,
                t.created_at, 
                tt.transaction_type_id AS "TransactionTypeId", 
                tt.name AS "Name"
            FROM transactions t
            INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
            INNER JOIN accounts a ON t.account_id = a.account_id
            INNER JOIN users u ON a.user_id = u.user_id
            WHERE u.google_id = @GoogleId;
            """;

            var transactions = await _dbConnection.QueryAsync<TransactionRequest, TransactionType, TransactionRequest>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    transaction.AccountNumber = transaction.AccountNumber;
                    return transaction;
                },
                new { GoogleId = googleId },
                splitOn: "TransactionTypeId"
            );

            // Log each transaction to the console
            foreach (var transaction in transactions)
            {
                Console.WriteLine($"Transaction ID: {transaction.AccountNumber}, Reference: {transaction.Reference}, Amount: {transaction.Amount}, Account Number: {transaction.AccountNumber}, Created At: {transaction.CreatedAt}");
            }

            return transactions;
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsByAccountNumberAsync(string accountNumber, string googleId)
        {
            string adminCheckQuery = """
            SELECT COUNT(1)
            FROM users
            WHERE google_id = @GoogleId AND role_id = 2;
            """;

            var isAdmin = await _dbConnection.ExecuteScalarAsync<bool>(adminCheckQuery, new { GoogleId = googleId });

            string query;

            if (isAdmin)
            {
                query = """
                SELECT 
                    t.transaction_id,
                    t.transaction_reference_id,
                    t.reference,
                    t.amount,
                    a.account_number AS "AccountNumber",
                    t.balance_after_transaction,
                    t.created_at, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON a.user_id = u.user_id
                WHERE a.account_number = @AccountNumber;
                """;
            }
            else
            {
                query = """
                SELECT 
                    t.transaction_id,
                    t.transaction_reference_id,
                    t.reference,
                    t.amount,
                     a.account_number AS "AccountNumber",
                    t.balance_after_transaction,
                    t.created_at, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON a.user_id = u.user_id
                WHERE u.google_id = @GoogleId AND a.account_number = @AccountNumber;
                """;
            }

            var transactions = await _dbConnection.QueryAsync<TransactionRequest, TransactionType, TransactionRequest>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    return transaction;
                },
                new { GoogleId = googleId, AccountNumber = accountNumber },
                splitOn: "TransactionTypeId"
            );

            return transactions;
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsByIdAsync(int transactionId, string googleId)
        {
            string adminCheckQuery = """
            SELECT COUNT(1)
            FROM users
            WHERE google_id = @GoogleId AND role_id = 2;
            """;

            var isAdmin = await _dbConnection.ExecuteScalarAsync<bool>(adminCheckQuery, new { GoogleId = googleId });

            string query;

            if (isAdmin)
            {
                query = """
                SELECT 
                    t.transaction_id,
                    t.transaction_reference_id,
                    t.reference,
                    t.amount,
                    t.balance_after_transaction,
                    t.created_at, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name",
                    a.account_number AS "AccountNumber"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON a.user_id = u.user_id
                WHERE t.transaction_id = @TransactionId;
                """;
            }
            else
            {
                query = """
                SELECT 
                    t.transaction_id,
                    t.transaction_reference_id,
                    t.reference,
                    t.amount,
                    t.balance_after_transaction,
                    t.created_at, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name",
                    a.account_number AS "AccountNumber"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON a.user_id = u.user_id
                WHERE u.google_id = @GoogleId AND t.transaction_id = @TransactionId;
                """;
            }

            var transactions = await _dbConnection.QueryAsync<TransactionRequest, TransactionType, TransactionRequest>(
                query,
                (transaction, transactionType) =>
                {
                    transaction.TransactionType = transactionType;
                    return transaction;
                },
                new { GoogleId = googleId, TransactionId = transactionId },
                splitOn: "TransactionTypeId"
            );

            return transactions;
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsByTransactionReferenceIdAsync(int transactionReferenceId, NpgsqlTransaction? tx = null)
        {
            string query = """
            SELECT 
                t.transaction_id,
                t.transaction_reference_id,
                t.reference,
                t.amount,
                t.balance_after_transaction,
                t.created_at, 
                tt.transaction_type_id AS "TransactionTypeId", 
                tt.name AS "Name",
                a.account_number AS "AccountNumber"
            FROM transactions t
            INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
            INNER JOIN accounts a ON t.account_id = a.account_id
            INNER JOIN users u ON a.user_id = u.user_id
            WHERE t.transaction_reference_id = @TransactionReferenceId;
            """;

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            await connection.OpenAsync();
            var transaction = tx ?? await connection.BeginTransactionAsync();

            try
            {
                var transactions = await _dbConnection.QueryAsync<TransactionRequest, TransactionType, TransactionRequest>(
                    query,
                    (transaction, transactionType) =>
                    {
                        transaction.TransactionType = transactionType;
                        return transaction;
                    },
                    new { TransactionReferenceId = transactionReferenceId },
                    splitOn: "TransactionTypeId"
                );

                if (tx == null)
                {
                    await transaction.CommitAsync();
                }

                return transactions;
            }
            catch
            {
                if (tx == null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tx == null)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> InsertReversalTransactions(IEnumerable<int> transactionIdsToReverse, NpgsqlTransaction? tx = null)
        {
            var query = """
            SELECT reverse_transactions(@TransactionIdsToReverse);
            """;

            var parameters = new
            {
                TransactionIdsToReverse = transactionIdsToReverse.ToArray()
            };

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            await connection.OpenAsync();
            var transaction = tx ?? await connection.BeginTransactionAsync();

            try
            {
                var insertedTransactionIds = await connection.QueryFirstOrDefaultAsync<bool>(
                    query,
                    parameters
                );

                if (tx == null)
                {
                    transaction.Commit();
                }

                return insertedTransactionIds;
            }
            catch
            {
                if (tx == null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (tx == null)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}