using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Api.Services;
using System.Threading.Tasks;
using System.Text.Json;

namespace Api.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly GoogleAuthService _googleAuthService;
        private readonly TokenService _tokenService;
 
        public AccountController(
            // ILogger<AuthController> logger,
            GoogleAuthService googleAuthService,
            TokenService tokenService)
        {
            // _logger = logger;
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
        }

        [Route("signin-google")]
        public IActionResult GoogleLogin()
        {
            // var token = _googleAuthService.ExchangeCodeForTokenAsync(accessCode);
            var properties = new AuthenticationProperties { RedirectUri = "https://localhost:7059/signin-google" };
            return Challenge(properties, GoogleOpenIdConnectDefaults.AuthenticationScheme);
        }

        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims.Select(claim => new {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value,
            });
            return new JsonResult(claims);
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

        [Route("token/{sessionId}")]
        public async Task<IActionResult> Token(string sessionId)
        {
            var token = await _googleAuthService.ExchangeCodeForTokenAsync(sessionId);
            return Ok(token);
        }
    }
}