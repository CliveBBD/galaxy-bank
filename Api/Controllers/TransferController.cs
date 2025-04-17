using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;

namespace Api.Controllers
{
    [Route("transfer")]
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payload = await JwtDecoder.Decode(HttpContext);
                if (payload == null)
                {
                    return Unauthorized("Invalid or missing token.");
                }
                var googleId = payload.Subject;
                var result = await _transferService.TransferAsync(request, googleId);
                await _emailService.SendEmailAsync(payload.Email, TransferEmailTemplate.Subject, TransferSenderEmailTemplate.Message(payload.GivenName, result.ReceiverName, request.Amount.ToString(), request.FromAccountNumber.ToString(), request.ToAccountNumber.ToString()));
                await _emailService.SendEmailAsync(result.ReceiverEmail, TransferEmailTemplate.Subject, TransferReceiverEmailTemplate.Message(payload.GivenName, result.ReceiverName, request.Amount.ToString(), request.FromAccountNumber.ToString(), request.ToAccountNumber.ToString()));
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}