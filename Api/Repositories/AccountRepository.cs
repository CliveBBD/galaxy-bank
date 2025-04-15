
using Api.DTOs;
using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountRepository
    {
        Task<int> CreateAccountAsync(AccountCreateRequest accountDto);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task<Account> GetAccountByIdAsync(int id);
        Task<IEnumerable<Account>> GetAccountsByUserEmailAsync(string email);

    }

    public class AccountRepository : IAccountRepository
    {
        public async Task<int> CreateAccountAsync(AccountCreateRequest accountDto)
        {
            var query = @"
                INSERT INTO accounts (user_id, account_type_id, balance)
                VALUES (@UserId, @AccountType, @Balance)
                RETURNING account_id;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var accountId = await connection.ExecuteScalarAsync<int>(query, accountDto);
            return accountId;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            var query = $@"
                SELECT account_id AS {nameof(Account.AccountId)}, user_id AS {nameof(Account.UserId)}, account_type_id AS {nameof(Account.AccountTypeId)}, balance AS {nameof(Account.Balance)}, created_at AS {nameof(Account.CreatedAt)}
                FROM accounts";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<Account>(query);
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
