using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("withdrawals")]
    public class WithdrawController : Controller
    {
        private readonly IWithdrawService _withdrawService;
        private readonly IEmailService _emailService;

        public WithdrawController(IWithdrawService withdrawService, IEmailService emailService)
        {
            _emailService = emailService;
            _withdrawService = withdrawService;
        }

        [HttpPost("", Name = "Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("Not authorized to withdraw", "You must be logged in to make a withdrawal", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var googleId = requestingUser.GoogleID;
                var result = await _withdrawService.WithdrawAsync(request, googleId);

                try
                {
                    _ = _emailService.SendEmailAsync(requestingUser.Email, WithdrawEmailTemplate.Subject, WithdrawEmailTemplate.Message(requestingUser.Username, request.AccountNumber.ToString(), request.Amount.ToString()));
                }
                catch
                {
                    // we do not need to wait for the email to be sent out and exceptions thrown by the email service should not cause the controller to fail
                }

                return Ok(result);
            }
        }
    }
}