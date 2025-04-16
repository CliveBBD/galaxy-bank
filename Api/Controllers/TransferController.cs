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

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost("", Name = "Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            Console.WriteLine("I'm here!");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Console.WriteLine("I'm here! 2");

            try
            {
                var payload = await JwtDecoder.Decode(HttpContext);
                if (payload == null)
                {
                    return Unauthorized("Invalid or missing token.");
                }
                var googleId = payload.Subject;
                var result = await _transferService.TransferAsync(request, googleId);
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