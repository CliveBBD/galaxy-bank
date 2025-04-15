using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
    }

    public class UserRepository : IUserRepository
    {

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var query = $@"
            SELECT 
                u.user_id AS {nameof(User.UserID)},
                u.google_id AS {nameof(User.GoogleID)},
                u.username AS {nameof(User.Username)},
                u.email AS {nameof(User.Email)},
                r.role_id AS RoleID,
                r.name AS Name
            FROM users u
            INNER JOIN roles r ON u.role_id = r.role_id
            WHERE u.user_id = @Id

        ";

            var userDict = new Dictionary<int, User>();

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var result = await connection.QueryAsync<User, Role, User>(
                query,
                (user, role) =>
                {
                    user.Role = role;
                    return user;
                },
                new { Id = id },
                splitOn: "RoleID"
            );

            return result.FirstOrDefault();
        }

    }
}
