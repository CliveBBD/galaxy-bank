using System.Data;
using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountTypeRepository
    {
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task<IEnumerable<AccountType>> GetAllAccountTypesAsync();
        Task<int> GetAccountTypeIdByNameAsync(string name);
    }
    public class AccountTypeRepository : IAccountTypeRepository
    {

        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {
            var query = $@"
                SELECT account_type_id AS AccountTypeId, name
                FROM account_types
                WHERE account_type_id = @Id;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<AccountType>(query, new { Id = id });
        }

        public async Task<int> GetAccountTypeIdByNameAsync(string name)
        {
            var query = $@"
                SELECT account_type_id AS AccountTypeId, name
                FROM account_types
                WHERE name = @Name;
                RETURNING account_type_id
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return (int)await connection.QueryFirstOrDefaultAsync<AccountType>(query, new { Name = name });
        }

        public async Task<IEnumerable<AccountType>> GetAllAccountTypesAsync()
        {
            var query = $@"SELECT account_type_id AS AccountTypeId, name FROM account_types;";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryAsync<AccountType>(query);
        }
    }
}
