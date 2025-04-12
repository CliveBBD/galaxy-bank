using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesAsync();
    }

  public class RoleService (IRoleRepository roleRepository) : IRoleService
  {
    private readonly IRoleRepository _roleRepository = roleRepository;
    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
      return await _roleRepository.GetRolesAsync();
    }
  }
}