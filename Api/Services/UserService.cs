using Api.Models;
using Api.Repositories;
using Google.Apis.Auth;

namespace Api.Services
{
    public interface IUserService
    {

        Task<int> CreateUserAsync(CreateUserDto dto);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetCurrentUser(HttpContext httpContext);

    }
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        private readonly IRoleService _roleService;
        public UserService(IUserRepository userRepository, IRoleService roleService)
        {
            _userRepository = userRepository;
            _roleService = roleService;
        }

        public async Task<int> CreateUserAsync(CreateUserDto dto)
        {
            bool exists = await _userRepository.UserExistsAsync(dto.GoogleID, dto.Email);

            if (exists)
            {
                throw new Exception("A user with the same GoogleID or Email already exists.");

            }

            return await _userRepository.CreateUserAsync(dto.GoogleID, dto.Username, dto.Email, dto.RoleName);

        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)

        {
            return await _userRepository.GetUserByEmailAsync(email);
        }


        public async Task<User?> GetCurrentUser(HttpContext httpContext)
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwt = authHeader.Substring("Bearer ".Length).Trim();
                var payload = await GoogleJsonWebSignature.ValidateAsync(jwt);
                return await this.GetUserByEmail(payload.Email);
            }
            else
            {
                return null;
            }
        }


    }
}