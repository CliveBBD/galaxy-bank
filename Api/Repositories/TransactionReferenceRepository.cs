using System.Data;
using Api.Models;
using Dapper;

namespace Api.Repositories
{
    public interface ITransactionReferenceRepository
    {
        public Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId);
    }

    public class TransactionReferenceRepository : ITransactionReferenceRepository
    {
        private readonly IDbConnection _dbConnection;
        public TransactionReferenceRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId)
        {
            // Query to get the user's role based on googleId
            string roleQuery = """
            SELECT role_id 
            FROM users 
            WHERE google_id = @GoogleId;
            """;

            // Fetch the user's role
            var roleId = await _dbConnection.QuerySingleOrDefaultAsync<int>(
                roleQuery,
                new { GoogleId = googleId }
            );

            // Check if the user is an admin (role_id = 2)
            if (roleId != 2)
            {
                throw new UnauthorizedAccessException("User is not authorized to access this data.");
            }

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