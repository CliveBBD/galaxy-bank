using System.Data;
using Api.Models;
using Api.Shared;
using Dapper;

namespace Api.Repositories
{
    public interface ITransactionReferenceRepository
    {
        public Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId);
        public Task<TransactionReference?> GetTransactionReferenceById(int transactionReferenceId);
    }

    public class TransactionReferenceRepository : ITransactionReferenceRepository
    {
        private readonly IDbConnection _dbConnection;
        public TransactionReferenceRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<TransactionReference?> GetTransactionReferenceById(int transactionReferenceId)
        {
            string query = """
                SELECT transaction_reference_id
                FROM transaction_references
                WHERE transaction_reference_id = @transactionReferenceId;
            """;

            return await _dbConnection.QuerySingleOrDefaultAsync<TransactionReference>(
                query,
                new { transactionReferenceId }
            );
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId)
        {
            // Query to get the user's role based on googleId
            string roleQuery = """
            SELECT r.name
            FROM users u
            INNER JOIN roles r ON u.role_id = r.role_id
            WHERE google_id = @GoogleId;
            """;

            // Fetch the user's role
            var roleName = await _dbConnection.QuerySingleOrDefaultAsync<string>(
                roleQuery,
                new { GoogleId = googleId }
            );

            // If user is not an admin, only fetch their transactions
            if (roleName != Constants.DisputeOfficerRoleName && roleName != Constants.SystemAdminRoleName)
            {
                // Query to fetch transactions
                string query = """
                SELECT 
                    t.*, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON u.user_id = a.user_id
                WHERE t.transaction_reference_id = @ReferenceId AND u.google_id = @GoogleId
                ;
                """;

                var transactions = await _dbConnection.QueryAsync<Transaction, TransactionType, Transaction>(
                    query,
                    (transaction, transactionType) =>
                    {
                        transaction.TransactionType = transactionType;
                        return transaction;
                    },
                    new
                    {
                        ReferenceId = referenceId,
                        GoogleId = googleId
                    },
                    splitOn: "TransactionTypeId"
                );

                return transactions;
            }
            else
            {
                // Query to fetch transactions
                string query = """
                SELECT 
                    t.*, 
                    tt.transaction_type_id AS "TransactionTypeId", 
                    tt.name AS "Name"
                FROM transactions t
                INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                WHERE t.transaction_reference_id = @ReferenceId;
                """;

                var transactions = await _dbConnection.QueryAsync<Transaction, TransactionType, Transaction>(
                    query,
                    (transaction, transactionType) =>
                    {
                        transaction.TransactionType = transactionType;
                        return transaction;
                    },
                    new { ReferenceId = referenceId }, // Pass the referenceId as a parameter
                    splitOn: "TransactionTypeId"
                );

                return transactions;
            }

        }
    }
}