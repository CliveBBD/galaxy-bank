using System.Data;
using Api.Interfaces;
using Api.Models;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnection _connectionString;

        public AccountRepository(IConfiguration configuration)
        {

            _connectionString = new NpgsqlConnection(configuration.GetConnectionString("GalaxyBankDB"));
        }

        public async Task<int> CreateAccountAsync(Account account)
        {
            var sql = @"
                INSERT INTO accounts (user_id, account_type_id, balance)
                VALUES (@UserId, @AccountTypeId, @Balance)
                RETURNING account_id;
            ";

            var accountId = await _connectionString.ExecuteScalarAsync<int>(sql, account);
            return accountId;
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            var sql = @"
                SELECT account_id, user_id, account_type_id, balance, created_at
                FROM accounts
                WHERE account_id = @Id";

            Account account = await _connectionString.QueryFirstOrDefaultAsync<Account>(sql, new { Id = id });
            return account;
        }


    }
}
