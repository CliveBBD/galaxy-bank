using Api.DTOs;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("", Name = "CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var currentUser = HttpContext.GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized(new ErrorResponse("Not authorized to create users", "You must be logged in and have appropriate permissions to create a user", StatusCodes.Status401Unauthorized));
            }
            else if (currentUser.Role.Name != Constants.SystemAdminRoleName)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Not authorized to create users", "You are not authorized to create a user", StatusCodes.Status403Forbidden));
            }
            else
            {
                var user = await _userService.CreateUserAsync(dto);
                return StatusCode(StatusCodes.Status201Created, user);
            }
        }

        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("Not authorized to view users", "You must be logged in to view users by user id", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ErrorResponse("User not found", $"Could not find a user with the specified id '{id}'", StatusCodes.Status404NotFound));
                else
                    return Ok(user);
            }
        }

        [HttpGet("by-email/{email}", Name = "GetUserByEmail")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("Not authorized to view users", "You must be logged in to view users by user id", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound(new ErrorResponse("User not found", $"Could not find a user with the specified email '{email}'", StatusCodes.Status404NotFound));
                else
                    return Ok(user);
            }
        }

    }
}
