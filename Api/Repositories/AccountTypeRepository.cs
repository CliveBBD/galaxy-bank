using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IAccountTypeRepository
    {
<<<<<<< HEAD
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task<IEnumerable<string>> GetAllAccountTypesAsync();
        Task<AccountType> GetAccountTypeByNameAsync(string name);
=======
        Task<AccountType?> GetAccountTypeByIdAsync(int id);
        Task<IEnumerable<AccountType>> GetAllAccountTypesAsync();
        Task<AccountType?> GetAccountTypeByNameAsync(string name);
>>>>>>> main
    }
    public class AccountTypeRepository : IAccountTypeRepository
    {

        public async Task<AccountType?> GetAccountTypeByIdAsync(int id)
        {
            var query = $@"
                SELECT account_type_id AS AccountTypeId, name
                FROM account_types
                WHERE account_type_id = @Id;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<AccountType>(query, new { Id = id });
        }

        public async Task<AccountType?> GetAccountTypeByNameAsync(string name)
        {
            var query = $@"
                SELECT account_type_id AS AccountTypeId, name
                FROM account_types
                WHERE name = @Name;
            ";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<AccountType>(query, new { Name = name });
        }

        public async Task<IEnumerable<string>> GetAllAccountTypesAsync()
        {
            var query = $@"SELECT name FROM account_types;";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryAsync<string>(query);
        }
    }
}
