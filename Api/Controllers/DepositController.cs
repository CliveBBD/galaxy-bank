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

        public DepositController(IDepositService depositService)
        {
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
                Console.WriteLine($"Google ID: {googleId}");

                var result = await _depositService.DepositAsync(request, googleId);
                Console.WriteLine(result);
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