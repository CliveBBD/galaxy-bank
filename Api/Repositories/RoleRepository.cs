using System.Data;
using System.Reflection.Metadata;
using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IRoleRepository
    {
        public Task<IEnumerable<Role>> GetRolesAsync();
        Task<Role?> GetRoleByNameAsync(string name);
    }

    public class RoleRepository : IRoleRepository
    {
        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            string query = """
            SELECT 
                role_id, 
                name
            FROM roles;
        """;

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            return await connection.QueryAsync<Role>(query);
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            var sql = "SELECT role_id AS RoleID, name FROM Roles WHERE Name = @Name";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });
        }
    }
}