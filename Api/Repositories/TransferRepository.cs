using System.Data;
using Api.DTOs;
using Dapper;

namespace Api.Repositories
{
    public interface ITransferRepository
    {
        public Task<(int TransactionResult, string ReceiverName, string ReceiverEmail)> TransferAsync(TransferRequest transferRequest, string googleId);
    }

    public class TransferRepository : ITransferRepository
    {
        private readonly IDbConnection _dbConnection;
        public TransferRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(int TransactionResult, string ReceiverName, string ReceiverEmail)> TransferAsync(TransferRequest transferRequest, string googleId)
        {
            if (transferRequest.Amount <= 0)
            {
                throw new ArgumentException("Transfer amount must be greater than zero.");
            }

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open(); // Ensure the connection is open
            }

            using var transaction = _dbConnection.BeginTransaction();
            try
            {
                // Step 1: Retrieve the sender's internal user ID using the Google ID
                string senderUserQuery = """
                SELECT user_id AS "UserId"
                FROM users
                WHERE google_id = @GoogleId;
                """;
                var senderUserId = await _dbConnection.ExecuteScalarAsync<int>(
                    senderUserQuery,
                    new { GoogleId = googleId },
                    transaction: transaction
                );

                // Step 2: Verify that the sender's account number belongs to the user
                string verifyAccountOwnershipQuery = """
                SELECT COUNT(1)
                FROM accounts
                WHERE account_number = @AccountNumber AND user_id = @UserId;
                """;
                int ownershipCount = await _dbConnection.ExecuteScalarAsync<int>(
                    verifyAccountOwnershipQuery,
                    new { AccountNumber = transferRequest.FromAccountNumber, UserId = senderUserId },
                    transaction: transaction
                );

                if (ownershipCount <= 0)
                {
                    throw new InvalidOperationException("The sender's account does not belong to the user.");
                }

                // Step 3: Retrieve the sender's current balance and account ID
                string senderAccountQuery = """
                SELECT account_id AS "AccountId", balance AS "CurrentBalance"
                FROM accounts
                WHERE account_number = @AccountNumber;
                """;
                var senderAccount = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    senderAccountQuery,
                    new { AccountNumber = transferRequest.FromAccountNumber },
                    transaction: transaction
                );

                if (senderAccount == null)
                {
                    throw new InvalidOperationException("No account found for the sender.");
                }

                int senderAccountId = senderAccount.AccountId;
                int senderCurrentBalance = senderAccount.CurrentBalance;

                // Step 4: Validate that the transfer amount does not exceed the sender's current balance
                if (transferRequest.Amount > senderCurrentBalance)
                {
                    throw new InvalidOperationException("Insufficient funds. Transfer amount exceeds the sender's current balance.");
                }

                // Step 5: Retrieve the receiver's account ID and current balance
                string receiverAccountQuery = """
                SELECT account_id AS "AccountId", balance AS "CurrentBalance"
                FROM accounts
                WHERE account_number = @AccountNumber;
                """;
                var receiverAccount = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    receiverAccountQuery,
                    new { AccountNumber = transferRequest.ToAccountNumber },
                    transaction: transaction
                );

                if (receiverAccount == null)
                {
                    throw new InvalidOperationException("No account found for the receiver.");
                }

                int receiverAccountId = receiverAccount.AccountId;
                int receiverCurrentBalance = receiverAccount.CurrentBalance;

                // Step 6: Deduct the amount from the sender's account
                string deductBalanceQuery = """
                UPDATE accounts
                SET balance = balance - @Amount
                WHERE account_number = @AccountNumber AND balance >= @Amount;
                """;
                int deductResult = await _dbConnection.ExecuteAsync(
                    deductBalanceQuery,
                    new { Amount = transferRequest.Amount, AccountNumber = transferRequest.FromAccountNumber },
                    transaction: transaction
                );

                if (deductResult <= 0)
                {
                    throw new InvalidOperationException("Failed to deduct the amount from the sender's account. Insufficient funds or account not found.");
                }

                // Step 7: Add the amount to the receiver's account
                string addBalanceQuery = """
                UPDATE accounts
                SET balance = balance + @Amount
                WHERE account_number = @AccountNumber;
                """;
                int addResult = await _dbConnection.ExecuteAsync(
                    addBalanceQuery,
                    new { Amount = transferRequest.Amount, AccountNumber = transferRequest.ToAccountNumber },
                    transaction: transaction
                );

                if (addResult <= 0)
                {
                    throw new InvalidOperationException("Failed to add the amount to the receiver's account.");
                }

                // Step 8: Insert a single transaction reference for both sender and receiver
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

                // Step 9: Insert the sender's transaction record
                string senderTransactionQuery = """
                INSERT INTO transactions (account_id, transaction_reference_id, transaction_type_id, amount, balance_after_transaction, created_at, reference)
                VALUES (@AccountId, @TransactionReferenceId, @TransactionTypeId, -@Amount, @BalanceAfterTransaction, @CreatedAt, @Reference);
                """;
                var senderTransactionParameters = new
                {
                    AccountId = senderAccountId,
                    TransactionReferenceId = transactionReferenceId,
                    TransactionTypeId = 3, // Assuming 3 represents "Transfer" in the transaction types table
                    Amount = transferRequest.Amount,
                    BalanceAfterTransaction = senderCurrentBalance - transferRequest.Amount,
                    CreatedAt = DateTime.UtcNow,
                    Reference = transferRequest.FromReference
                };

                int senderTransactionResult = await _dbConnection.ExecuteAsync(senderTransactionQuery, senderTransactionParameters, transaction: transaction);

                if (senderTransactionResult <= 0)
                {
                    throw new InvalidOperationException("Failed to insert the sender's transaction record.");
                }

                // Step 10: Insert the receiver's transaction record
                string receiverTransactionQuery = """
                INSERT INTO transactions (account_id, transaction_reference_id, transaction_type_id, amount, balance_after_transaction, created_at, reference)
                VALUES (@AccountId, @TransactionReferenceId, @TransactionTypeId, @Amount, @BalanceAfterTransaction, @CreatedAt, @Reference);
                """;
                var receiverTransactionParameters = new
                {
                    AccountId = receiverAccountId,
                    TransactionReferenceId = transactionReferenceId,
                    TransactionTypeId = 4, // Assuming 4 represents "Receive" in the transaction types table
                    Amount = transferRequest.Amount,
                    BalanceAfterTransaction = receiverCurrentBalance + transferRequest.Amount,
                    CreatedAt = DateTime.UtcNow,
                    Reference = transferRequest.ToReference
                };

                int receiverTransactionResult = await _dbConnection.ExecuteAsync(receiverTransactionQuery, receiverTransactionParameters, transaction: transaction);

                if (receiverTransactionResult <= 0)
                {
                    throw new InvalidOperationException("Failed to insert the receiver's transaction record.");
                }

                // Step 11: Retrieve the receiver's name and email
                string receiverDetailsQuery = """
                SELECT u.username AS "Name", u.email AS "Email"
                FROM users u
                INNER JOIN accounts a ON u.user_id = a.user_id
                WHERE a.account_number = @AccountNumber;
                """;
                var receiverDetails = await _dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                    receiverDetailsQuery,
                    new { AccountNumber = transferRequest.ToAccountNumber },
                    transaction: transaction
                );

                if (receiverDetails == null)
                {
                    throw new InvalidOperationException("Failed to retrieve the receiver's details.");
                }

                string receiverName = receiverDetails.Name;
                string receiverEmail = receiverDetails.Email;

                transaction.Commit();
                return (senderTransactionResult + receiverTransactionResult, receiverName, receiverEmail);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException($"An error occurred while processing the transfer: {ex.Message}", ex);
            }
            finally
            {
                transaction.Dispose();
            }
        }
    }
}
