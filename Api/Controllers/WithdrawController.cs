using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;

namespace Api.Controllers
{
    [Route("withdraw")]
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

                var result = await _withdrawService.WithdrawAsync(request, googleId);
                await _emailService.SendEmailAsync(payload.Email, WithdrawEmailTemplate.Subject, WithdrawEmailTemplate.Message(payload.GivenName, request.AccountNumber.ToString(), request.Amount.ToString()));
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