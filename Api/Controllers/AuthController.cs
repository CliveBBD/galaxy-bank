using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Shared;
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
            TokenService tokenService
            )
        {
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
        }

        [Route("signin-google")]
        public async Task<ContentResult> GoogleLogin([FromQuery] string code, [FromQuery] string state)
        {
            if(string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state)) 
            {
                return new ContentResult
                {
                    Content = "<h2>Either state or code is null. Please try authenticating again!</h2>",
                    ContentType = "text/html",
                    StatusCode = 200
                };
            }
            try
            {
                var token = await _googleAuthService.ExchangeCodeForTokenAsync(code, state);
            if(token != null) { _tokenService.StoreToken(state, token); }
            var html = @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
                <title>Authentication Successful</title>
                <style>
                    body {
                    font-family: sans-serif;
                    background-color: #f4f4f4;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    margin: 0;
                    }

                    .section-container {
                    position: relative;
                    background-color: white;
                    border: 1px solid #ccc;
                    border-radius: 8px;
                    padding: 2rem 1.5rem 1.5rem;
                    max-width: 400px;
                    text-align: center;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                    }

                    .check-circle {
                    position: absolute;
                    top: -16px;
                    left: 50%;
                    transform: translateX(-50%);
                    background-color: #28a745;
                    color: white;
                    width: 32px;
                    height: 32px;
                    border-radius: 50%;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    font-size: 18px;
                    box-shadow: 0 0 0 2px white;
                    }

                    .message {
                    margin-top: 10px;
                    font-size: 16px;
                    color: #333;
                    }
                </style>
                </head>
                <body>
                <div class=""section-container"">
                    <div class=""check-circle"">✔</div>
                    <div class=""message"">
                    Authentication successful, you can close this tab and enjoy our CLI.
                    </div>
                </div>
                </body>
                </html>
                ";
                return new ContentResult
                {
                    Content = html,
                    ContentType = "text/html",
                    StatusCode = 200
                };
            }
            catch(Exception ex)
            {
                return new ContentResult
                {
                    Content = $"<h2>Authentication service unavailable: {ex}></h2>",
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
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
                return Ok(token);
            }
        }

        [HttpPost("logout")]
        public IActionResult LogOut([FromForm] string sessionId)
        {
            var logout = _tokenService.RemoveToken(sessionId);
            return Ok(logout);
        }
    }
}