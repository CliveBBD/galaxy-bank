using System.Data;
using Api.DTOs;
using Dapper;

namespace Api.Repositories
{
    public interface IDepositRepository
    {
        public Task<int> DepositAsync(DepositRequest depositRequest, string googleId);
    }

    public class DepositRepository : IDepositRepository
    {
        private readonly IDbConnection _dbConnection;
        public DepositRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<int> DepositAsync(DepositRequest depositRequest, string googleId)
        {
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open(); // Ensure the connection is open
            }

            using var transaction = _dbConnection.BeginTransaction();
            try
            {
                // Step 1: Retrieve the internal user ID using the Google ID
                string userQuery = """
                SELECT user_id AS "UserId"
                FROM users
                WHERE google_id = @GoogleId;
                """;
                var user = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    userQuery,
                    new { GoogleId = googleId },
                    transaction: transaction
                );


                if (user == null)
                {
                    throw new InvalidOperationException($"No user found with the Google ID '{googleId}'.");
                }

                int userId = user.UserId;

                // Step 2: Insert a new transaction reference
                string transactionReferenceQuery = """
                INSERT INTO transaction_references DEFAULT VALUES RETURNING transaction_reference_id;
                """;
                int transactionReferenceId = await _dbConnection.ExecuteScalarAsync<int>(
                    transactionReferenceQuery,
                    transaction: transaction
                );


                if (transactionReferenceId <= 0)
                {
                    throw new InvalidOperationException("Failed to create a transaction reference.");
                }

                // Step 3: Confirm the provided account number belongs to the user
                string accountValidationQuery = """
                SELECT account_id AS "AccountId", balance AS "CurrentBalance"
                FROM accounts
                WHERE account_number = @AccountNumber AND user_id = @UserId;
                """;
                var account = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    accountValidationQuery,
                    new { AccountNumber = depositRequest.AccountNumber, UserId = userId },
                    transaction: transaction
                );


                if (account == null)
                {
                    throw new InvalidOperationException($"The account with number '{depositRequest.AccountNumber}' does not belong to the user with ID '{userId}' or does not exist.");
                }

                int accountId = account.AccountId; // Retrieve the account ID for further operations
                int currentBalance = account.CurrentBalance;

                // Step 4: Calculate the new balance after the deposit
                int newBalance = currentBalance + depositRequest.Amount;

                // Step 5: Insert the transaction record
                string transactionQuery = """
                INSERT INTO transactions (account_id, transaction_reference_id, transaction_type_id, amount, balance_after_transaction, created_at, reference)
                VALUES (@AccountId, @TransactionReferenceId, @TransactionTypeId, @Amount, @BalanceAfterTransaction, @CreatedAt, @Reference);
                """;
                var transactionParameters = new
                {
                    AccountId = accountId, // Use the retrieved account ID
                    TransactionReferenceId = transactionReferenceId,
                    TransactionTypeId = 1, // Assuming 1 represents "Deposit" in the transaction types table
                    Amount = depositRequest.Amount,
                    BalanceAfterTransaction = newBalance,
                    CreatedAt = DateTime.UtcNow,
                    Reference = depositRequest.Reference // Assuming the reference is part of the request
                };


                int rowsAffected = await _dbConnection.ExecuteAsync(transactionQuery, transactionParameters, transaction: transaction);


                if (rowsAffected <= 0)
                {
                    throw new InvalidOperationException("Failed to insert the transaction record.");
                }

                // Step 6: Update the account balance
                string updateBalanceQuery = """
                UPDATE accounts
                SET balance = @NewBalance
                WHERE account_id = @AccountId;
                """;
                int balanceUpdateResult = await _dbConnection.ExecuteAsync(
                    updateBalanceQuery,
                    new { NewBalance = newBalance, AccountId = accountId },
                    transaction: transaction
                );

                if (balanceUpdateResult <= 0)
                {
                    throw new InvalidOperationException($"Failed to update the balance for account number '{depositRequest.AccountNumber}'.");
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"An error occurred while processing the deposit: {ex.Message}", ex);
            }
        }
    }
}
