
using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountRepository
    {
        Task<string> CreateAccountAsync(string accountTypeName);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task<Account> GetAccountByIdAsync(string id);
        Task<IEnumerable<Account>> GetAccountsByUserEmailAsync(string email);

    }

    public class AccountRepository : IAccountRepository
    {
        private readonly IAccountTypeRepository _accountTypeRepository;
        private IUserRepository _userRepository;

        public AccountRepository(IAccountTypeRepository accountTypeRepository, IUserRepository userRepository)
        {
            _accountTypeRepository = accountTypeRepository;
            _userRepository = userRepository;

        }
        public async Task<string> CreateAccountAsync(string accountTypeName)
        {
            var newUser = new CreateUserDto
            {
                GoogleID = "google-oath2|6679",
                Username = "newuser",
                Email = "testmail@email.com",
                RoleName = "customer"
            };


            var openingBalance = 50;
            var accountType = await _accountTypeRepository.GetAccountTypeByNameAsync(accountTypeName);

            if (accountType == null)
                throw new ArgumentException($"Account type {accountTypeName} does not exist, available account types are 'checking', 'savings' and 'credit_card'.");

            bool exists = await _userRepository.UserExistsAsync(newUser.GoogleID, newUser.Email);

            int userId;
            if (!exists)
            {
                userId = await _userRepository.CreateUserAsync(newUser.GoogleID, newUser.Username, newUser.Email, newUser.RoleName);
            }
            else
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
                if (existingUser == null)
                    throw new Exception("User exists but could not be retrieved.");

                userId = existingUser.UserID;
            }

            var query = @"
                INSERT INTO accounts (user_id, account_type_id, balance)
                VALUES (@UserId, @AccountType, @Balance)
                RETURNING account_number;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var accountNumber = await connection.ExecuteScalarAsync<string>(query, new
            {
                UserId = userId,
                AccountType = accountType.AccountTypeId,
                Balance = openingBalance
            }
             );

            return accountNumber;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}, account_number AS {nameof(Account.AccountNumber)}
                FROM accounts";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<Account>(query);
        }

        public async Task<Account> GetAccountByIdAsync(string id)
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}, account_number AS {nameof(Account.AccountNumber)}
                FROM accounts
                WHERE account_number = @Id";

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
                    a.created_at AS {nameof(Account.CreatedAt)},
                    a.account_number AS {nameof(Account.AccountNumber)}
                    FROM accounts a
                    INNER JOIN Users u ON a.user_Id = u.user_Id
                    WHERE u.Email = @Email";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            return await connection.QueryAsync<Account>(query, new { Email = email });
        }

    }
}
