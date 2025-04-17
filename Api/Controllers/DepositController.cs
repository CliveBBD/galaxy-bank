using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Shared;

namespace Api.Controllers
{
    [Route("deposit")]
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
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
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

                var result = await _depositService.DepositAsync(request, googleId);
                await _emailService.SendEmailAsync(payload.Email, DepositEmailTemplate.Subject, DepositEmailTemplate.Message(payload.GivenName, request.AccountNumber.ToString(), request.Amount.ToString()));
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