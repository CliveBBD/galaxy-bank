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
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly IDbConnection _dbConnection;
        public RoleRepository(IDbConnection? dbConnection)
        {
            //TODO: undo this patch and fix it properly
            _dbConnection = new NpgsqlConnection(Constants.ConnectionString);
        }
        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            string query = """
            SELECT 
                role_id, 
                name
            FROM roles;
        """;
            return await _dbConnection.QueryAsync<Role>(query);
        }
    }
}