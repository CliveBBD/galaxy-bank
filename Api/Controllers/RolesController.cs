using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("roles")]
    public class RolesController(IRoleService roleService) : Controller
    {
        private readonly IRoleService _roleService = roleService;

        [HttpGet("", Name = "GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();
                Console.WriteLine(roles);
                return Ok(roles);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{name}", Name = "GetRoleByName")]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            var role = await _roleService.GetRoleByNameAsync(name);

            if (role == null)
                return NotFound(new { message = $"Role '{name}' not found." });

            return Ok(role);
        }

    }
}