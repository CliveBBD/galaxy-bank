using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
    public interface IUserRepository
    {

        Task<int> CreateUserAsync(string googleID, string username, string email, string roleName);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string googleId, string email);


    }

    public class UserRepository : IUserRepository
    {
        private readonly IRoleRepository _roleRepository;

        public UserRepository(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;

        }


        public async Task<int> CreateUserAsync(string googleID, string username, string email, string roleName)
        {
            var role = await _roleRepository.GetRoleByNameAsync(roleName);

            if (role == null)
                throw new ArgumentException($"Role {roleName} does not exist.");

            var sql = @"
            INSERT INTO Users (google_id, username, email, role_id)
            VALUES (@GoogleID, @Username, @Email, @RoleName)
            RETURNING user_id";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var userId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                GoogleID = googleID,
                Username = username,
                Email = email,
                RoleName = role.RoleID,

            }
             );
            return userId;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var sql = $@"
            SELECT u.user_id AS {nameof(User.UserID)},
                u.google_id AS {nameof(User.GoogleID)},
                u.username AS {nameof(User.Username)},
                u.email AS {nameof(User.Email)}, r.role_id AS RoleID, r.name
            FROM users u
            INNER JOIN roles r ON u.role_id = r.role_id
            WHERE u.user_id = @UserID";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var results = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) => { user.Role = role; return user; },
                new { UserID = userId },
                splitOn: "RoleID"
            );

            return results.FirstOrDefault();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var sql = $@"
            SELECT u.user_id AS {nameof(User.UserID)},
                u.google_id AS {nameof(User.GoogleID)},
                u.username AS {nameof(User.Username)},
                u.email AS {nameof(User.Email)}, r.role_id AS RoleID, r.name
            FROM users u
            INNER JOIN roles r ON u.role_id = r.role_id
            WHERE u.email = @Email";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);

            var results = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) => { user.Role = role; return user; },
                new { Email = email },
                splitOn: "RoleID"
            );

            return results.FirstOrDefault();
        }
        public async Task<bool> UserExistsAsync(string googleId, string email)
        {
            var sql = @"SELECT 1 FROM users 
                WHERE google_id = @GoogleID 
                   OR email = @Email
                LIMIT 1";

            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new
            {
                GoogleID = googleId,
                Email = email
            });

            return result.HasValue;
        }

    }
}
