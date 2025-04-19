using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Models;
using Api.DTOs;

namespace Api.Controllers
{
    [ApiController]
    [Route("")]

    public class AccountController : ControllerBase
    {
        private readonly GoogleAuthService _googleAuthService;
        private readonly TokenService _tokenService;
 
        public AccountController(
            GoogleAuthService googleAuthService,
            TokenService tokenService)
        {
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
        }

        [Route("signin-google")]
        public async Task<IActionResult> GoogleLogin([FromQuery] string code, [FromQuery] string state)
        {
            var token = await _googleAuthService.ExchangeCodeForTokenAsync(code);
            if (token != null)
            {
                _tokenService.StoreToken(state, token);
                return Ok(token);
            }
            else
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse("Authentication service unavailable", "The authentication service is currently unavailable. Please try again later.", StatusCodes.Status503ServiceUnavailable));
            }
        }

        [Route("login")]
        public IActionResult Login()
        {
            // Generate a session ID to track this auth flow
            var sessionId = _googleAuthService.GenerateSessionId();
            
            // Generate the Google OAuth URL
            var authUrl = _googleAuthService.GenerateAuthUrl(sessionId);

            // Return the auth URL and session ID to the client
            return Ok(new LoginResponse(authUrl, sessionId));
        }

        [HttpGet("token/{sessionId}")]
        public IActionResult GetToken(string sessionId)
        {
            var token = _tokenService.GetToken(sessionId);

            if (token == null)
            {
                return NotFound(new ErrorResponse("Session token not found", "No token found for this session.", StatusCodes.Status404NotFound));
            }
            else
            {
                return Ok(new StoredToken
                {
                    IdToken = token.IdToken
                });
            }

        }
    }
}