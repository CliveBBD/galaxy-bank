using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountRepository
    {
        Task<string?> CreateAccountAsync(string accountTypeName, CreateUserDto userDto);
        Task<IEnumerable<Account>> GetAccountsAsync(string googleId);
        Task<IEnumerable<Account>> GetAccountsAsync(int? userId = null);
        Task<Account?> GetAccountByAccountNumberAsync(string accountNumber);
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
        public async Task<string?> CreateAccountAsync(string accountTypeName, CreateUserDto userDto)
        {
            var newUser = userDto;


            var openingBalance = 0;
            var accountType = await _accountTypeRepository.GetAccountTypeByNameAsync(accountTypeName);

            if (accountType == null)
                throw new ArgumentException($"Account type {accountTypeName} does not exist, available account types are 'checking', 'savings' and 'credit_card'.");

            bool exists = await _userRepository.UserExistsAsync(newUser.GoogleID, newUser.Email);

            User? user;
            if (!exists)
            {
                user = await _userRepository.CreateUserAsync(newUser.GoogleID, newUser.Username, newUser.Email, "customer");
            }
            else
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(newUser.Email);
                if (existingUser == null)
                    throw new Exception("User exists but could not be retrieved.");

                user = existingUser;
            }

            var query = @"
                INSERT INTO accounts (user_id, account_type_id, balance)
                VALUES (@UserId, @AccountType, @Balance)
                RETURNING account_number;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var accountNumber = await connection.ExecuteScalarAsync<string>(
                query,
                new
                {
                    UserId = user,
                    AccountType = accountType.AccountTypeId,
                    Balance = openingBalance
                }
            );

            return accountNumber;
        }
        
        public async Task<IEnumerable<Account>> GetAccountsAsync(string googleId)
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
                    WHERE u.google_id = @GoogleId";
            try
            {
                using var connection = new NpgsqlConnection(Constants.ConnectionString);
                await connection.OpenAsync();
                return await connection.QueryAsync<Account>(query, new { GoogleId = googleId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching accounts: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync(int? userId = null)
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}, account_number AS {nameof(Account.AccountNumber)}
                FROM accounts
                WHERE (@userId IS NULL OR user_id = @userId)
            ";

            var parameters = new
            {
                userId
            };

            try
            {
                using var connection = new NpgsqlConnection(Constants.ConnectionString);
                return await connection.QueryAsync<Account>(query, param: parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching accounts: {ex.Message}");
                throw;
            }
        }

        public async Task<Account?> GetAccountByAccountNumberAsync(string accountNumber)
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}, account_number AS {nameof(Account.AccountNumber)}
                FROM accounts
                WHERE account_number = @AccountNumber";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            return await connection.QueryFirstOrDefaultAsync<Account>(query, new { AccountNumber = accountNumber });
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
