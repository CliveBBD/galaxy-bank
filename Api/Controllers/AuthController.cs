using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GoogleAuthService _googleAuthService;
        private readonly TokenService _tokenService;
 
        public AccountController(
            GoogleAuthService googleAuthService,
            TokenService tokenService
            )
        {
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [Route("signin-google")]
        public async Task<IActionResult> GoogleLogin([FromQuery] string code, [FromQuery] string state)
        {
            var token = await _googleAuthService.ExchangeCodeForTokenAsync(code, state);
            if(token != null) { _tokenService.StoreToken(state, token); }
            return Ok(token);
        }

        [AllowAnonymous]
        [Route("login")]
        public IActionResult Login()
        {
            
            // Generate a session ID to track this auth flow
            var sessionId = _googleAuthService.GenerateSessionId();
            
            // Generate the Google OAuth URL
            var authUrl = _googleAuthService.GenerateAuthUrl(sessionId);
            
            // Return the auth URL and session ID to the client
            return Ok(new
            {
                authUrl,
                sessionId
            });
        }

        [AllowAnonymous]
        [HttpGet("token/{sessionId}")]
        public IActionResult GetToken(string sessionId)
        {
            var token = _tokenService.GetToken(sessionId);

            if (token == null)
            {
                return NotFound("No token found for this session.");
            }

            // Console.WriteLine(payload.Email);
            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult LogOut([FromForm] string sessionId)
        {
            var logout = _tokenService.RemoveToken(sessionId);
            return Ok(logout);
        }
    }
}