using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("transfers")]
    public class TransferController : Controller
    {
        private readonly ITransferService _transferService;
        private readonly IEmailService _emailService;

        public TransferController(ITransferService transferService, IEmailService emailService)
        {
            _emailService = emailService;
            _transferService = transferService;
        }

        [HttpPost("", Name = "Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized("Invalid or missing token.");
            }
            else
            {
                var googleId = requestingUser.GoogleID;
                var result = await _transferService.TransferAsync(request, googleId);

                try
                {
                    _ = _emailService.SendEmailAsync(requestingUser.Email, TransferSenderEmailTemplate.Subject, TransferSenderEmailTemplate.Message(requestingUser.Username, result.ReceiverName, request.Amount.ToString(), request.FromAccountNumber.ToString(), request.ToAccountNumber.ToString()));
                    _ = _emailService.SendEmailAsync(result.ReceiverEmail, TransferReceiverEmailTemplate.Subject, TransferReceiverEmailTemplate.Message(requestingUser.Username, result.ReceiverName, request.Amount.ToString(), request.FromAccountNumber.ToString(), request.ToAccountNumber.ToString()));
                }
                catch
                {
                    // errors thrown by the email service shouldn't affect the regular flow
                }

                return StatusCode(StatusCodes.Status201Created, result);
            }
        }
    }
}