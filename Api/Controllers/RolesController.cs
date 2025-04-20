using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("roles")]
    public class RolesController(IRoleService roleService, IUserService userService) : Controller
    {
        private readonly IRoleService _roleService = roleService;
        private readonly IUserService _userService = userService;

        [HttpGet("", Name = "GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser != null && (requestingUser.Role.Name == Constants.DisputeOfficerRoleName || requestingUser.Role.Name == Constants.SystemAdminRoleName))
            {
                var roles = await _roleService.GetRolesAsync();
                return Ok(roles);
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Resource access forbidden", "Accessing this resource is forbidden", StatusCodes.Status403Forbidden));
            }
        }

        [HttpGet("{name}", Name = "GetRoleByName")]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser != null && (requestingUser.Role.Name == Constants.DisputeOfficerRoleName || requestingUser.Role.Name == Constants.SystemAdminRoleName))
            {
                var role = await _roleService.GetRoleByNameAsync(name);
                if (role == null) return NotFound(new ErrorResponse("Role not found", $"Role '{name}' not found.", StatusCodes.Status404NotFound));
                else return Ok(role);
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Accessing resource forbidden", "Accessing this resource is forbidden", StatusCodes.Status403Forbidden));
            }
        }
    }
}