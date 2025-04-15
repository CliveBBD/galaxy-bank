using System.Data;
using Api.Models;
using Dapper;

namespace Api.Repositories
{
    public interface IRoleRepository
    {
        public Task<IEnumerable<Role>> GetRolesAsync();
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly IDbConnection _dbConnection;
        public RoleRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
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