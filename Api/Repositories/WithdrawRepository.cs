using System.Data;
using Api.DTOs;
using Dapper;

namespace Api.Repositories
{
    public interface IWithdrawRepository
    {
        public Task<int> WithdrawAsync(WithdrawRequest withdrawRequest, string googleId);
    }

    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly IDbConnection _dbConnection;
        public WithdrawRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<int> WithdrawAsync(WithdrawRequest withdrawRequest, string googleId)
        {
            if (withdrawRequest.Amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }

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
                    throw new InvalidOperationException("No user found with the provided Google ID.");
                }

                int userId = user.UserId;

                // Step 2: Validate that the account belongs to the user
                string accountValidationQuery = """
                SELECT COUNT(1)
                FROM accounts
                WHERE account_id = @AccountId AND user_id = @UserId;
                """;
                int accountExists = await _dbConnection.ExecuteScalarAsync<int>(
                    accountValidationQuery,
                    new { AccountId = withdrawRequest.AccountId, UserId = userId },
                    transaction: transaction
                );

                if (accountExists <= 0)
                {
                    throw new InvalidOperationException("The specified account does not belong to the user or is invalid.");
                }

                // Step 3: Retrieve the current balance for the account
                string balanceQuery = """
                SELECT balance AS "CurrentBalance"
                FROM accounts
                WHERE account_id = @AccountId;
                """;
                var account = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    balanceQuery,
                    new { AccountId = withdrawRequest.AccountId },
                    transaction: transaction
                );

                if (account == null)
                {
                    throw new InvalidOperationException("Account not found.");
                }

                int currentBalance = account.CurrentBalance;

                // Step 4: Validate that the withdrawal amount does not exceed the current balance
                if (withdrawRequest.Amount > currentBalance)
                {
                    throw new InvalidOperationException("Insufficient funds. Withdrawal amount exceeds the current balance.");
                }

                // Step 5: Update the account balance atomically
                string updateBalanceQuery = """
                UPDATE accounts
                SET balance = balance - @Amount
                WHERE account_id = @AccountId AND balance >= @Amount;
                """;
                int balanceUpdateResult = await _dbConnection.ExecuteAsync(
                    updateBalanceQuery,
                    new { Amount = withdrawRequest.Amount, AccountId = withdrawRequest.AccountId },
                    transaction: transaction
                );

                if (balanceUpdateResult <= 0)
                {
                    throw new InvalidOperationException("Failed to update the account balance. Insufficient funds or account not found.");
                }

                // Step 6: Insert a new transaction reference
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

                // Step 7: Insert the transaction record
                string transactionQuery = """
                INSERT INTO transactions (account_id, transaction_reference_id, transaction_type_id, amount, balance_after_transaction, created_at, reference)
                VALUES (@AccountId, @TransactionReferenceId, @TransactionTypeId, -@Amount, @BalanceAfterTransaction, @CreatedAt, @Reference);
                """;
                var transactionParameters = new
                {
                    AccountId = withdrawRequest.AccountId,
                    TransactionReferenceId = transactionReferenceId,
                    TransactionTypeId = 1, // Assuming 1 represents "Withdraw" in the transaction types table
                    Amount = withdrawRequest.Amount,
                    BalanceAfterTransaction = currentBalance - withdrawRequest.Amount,
                    CreatedAt = DateTime.UtcNow,
                    Reference = withdrawRequest.Reference
                };

                int rowsAffected = await _dbConnection.ExecuteAsync(transactionQuery, transactionParameters, transaction: transaction);

                if (rowsAffected <= 0)
                {
                    throw new InvalidOperationException("Failed to insert the transaction record.");
                }

                transaction.Commit();
                return rowsAffected;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"An error occurred while processing the withdrawal: {ex.Message}", ex);
            }
            finally
            {
                transaction.Dispose();
            }
        }
    }
}
