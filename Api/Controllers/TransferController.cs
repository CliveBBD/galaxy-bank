using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;

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

            string googleId = "google_id_b597e0d6d8e47ab0405e4627"; // Replace with actual Google ID from context.
            try
            {
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