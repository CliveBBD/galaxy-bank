using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IUserService
    {
        Task<User> GetUserById(int id);
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


    }
}