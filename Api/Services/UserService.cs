using Api.Models;
using Api.Repositories;
using Google.Apis.Auth;

namespace Api.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(int id);
        Task<User> GetUserByEmail(string email);
        Task<User?> GetCurrentUser(HttpContext httpContext);
    }
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User?> GetUserById(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmail(string email)
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