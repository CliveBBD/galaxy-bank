using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountRepository
    {
        Task<int> CreateAccountAsync(int userId, string accountTypeName);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task<Account> GetAccountByIdAsync(int id);
        Task<IEnumerable<Account>> GetAccountsByUserEmailAsync(string email);

    }

    public class AccountRepository : IAccountRepository
    {
        private readonly IAccountTypeRepository _accountTypeRepository;

        public AccountRepository(IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeRepository = accountTypeRepository;

        }
        public async Task<int> CreateAccountAsync(int userId, string accountTypeName)
        {
            var openingBalance = 50;
            var accountType = await _accountTypeRepository.GetAccountTypeByNameAsync(accountTypeName);

            if (accountType == null)
                throw new ArgumentException($"Account type {accountTypeName} does not exist, available account types are 'checking', 'savings' and 'credit_card'.");

            var query = @"
                INSERT INTO accounts (user_id, account_type_id, balance)
                VALUES (@UserId, @AccountType, @Balance)
                RETURNING account_id;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var accountId = await connection.ExecuteScalarAsync<int>(query, new
            {
                UserId = userId,
                AccountType = accountType.AccountTypeId,
                Balance = openingBalance
            }
             );

            return accountId;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}
                FROM accounts";
            Console.WriteLine(query);

            try
            {
                using var connection = new NpgsqlConnection(Constants.ConnectionString);
                Console.WriteLine(connection.ConnectionString);
                await connection.OpenAsync();
                return await connection.QueryAsync<Account>(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching accounts: {ex.Message}");
                throw;
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}
                FROM accounts
                WHERE account_id = @Id";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            return await connection.QueryFirstOrDefaultAsync<Account>(query, new { Id = id });
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserEmailAsync(string email)
        {
            var query = $@"
                    SELECT a.account_id AS {nameof(Account.AccountId)},
                    a.user_id AS {nameof(Account.UserId)},
                    a.account_type_id AS {nameof(Account.AccountTypeId)},
                    a.balance AS {nameof(Account.Balance)},
                    a.created_at AS {nameof(Account.CreatedAt)}
                    FROM accounts a
                    INNER JOIN Users u ON a.user_Id = u.user_Id
                    WHERE u.Email = @Email";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            return await connection.QueryAsync<Account>(query, new { Email = email });
        }

    }
}
