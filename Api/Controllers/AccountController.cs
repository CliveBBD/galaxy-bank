using Microsoft.AspNetCore.Mvc;
using Api.Services;

namespace Api.Controllers
{
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
            _tokenService.StoreToken(state, token);
            return Ok(token);
        }

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

        [HttpGet("token/{sessionId}")]
        public IActionResult GetToken(string sessionId)
        {
            var token = _tokenService.GetToken(sessionId);

            if (token == null)
            {
                return NotFound("No token found for this session.");
            }

            return Ok(new
            {
                accessToken = token.AccessToken,
                expiresAt = token.ExpiresAt
            });
        }
    }
}