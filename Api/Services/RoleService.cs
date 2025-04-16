using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesAsync();
        Task<Role?> GetRoleByNameAsync(string name);
    }

  public class RoleService (IRoleRepository roleRepository) : IRoleService
  {
    private readonly IRoleRepository _roleRepository = roleRepository;
    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
      return await _roleRepository.GetRolesAsync();
    }

    public Task<Role?> GetRoleByNameAsync(string name)
    {
        return _roleRepository.GetRoleByNameAsync(name);
    }
  }
}