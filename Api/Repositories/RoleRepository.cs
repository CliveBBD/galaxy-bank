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
    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        string query = $"""
            SELECT 
                role_id AS { nameof(Role.RoleID) }, 
                name AS { nameof(Role.Name) }
            FROM roles;
        """;

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        return await connection.QueryAsync<Role>(query);
    }
  }
}