using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("deposits")]
    public class DepositController : Controller
    {
        private readonly IDepositService _depositService;
        private readonly IEmailService _emailService;

        public DepositController(IDepositService depositService, IEmailService emailService)
        {
            _emailService = emailService;
            _depositService = depositService;
        }

        [HttpPost("", Name = "Deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest depositRequest)
        {
            var currentUser = HttpContext.GetCurrentUser();

            if (currentUser == null)
            {
                return Unauthorized(new ErrorResponse("User not authenticated", "You need to be logged in to make a deposit", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var result = await _depositService.DepositAsync(depositRequest, currentUser.GoogleID);
                try
                {
                    _ = _emailService.SendEmailAsync(currentUser.Email, DepositEmailTemplate.Subject, DepositEmailTemplate.Message(currentUser.Username, depositRequest.AccountNumber.ToString(), depositRequest.Amount.ToString()));
                }
                catch
                {
                    // errors thrown by the emailing services should not cause the request to fail
                }

                return StatusCode(StatusCodes.Status201Created, result);
            }
        }
    }
}