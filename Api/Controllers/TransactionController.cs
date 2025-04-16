using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.Shared;

namespace Api.Controllers
{
    [Route("transactions")]
    public class TransactionsController(ITransactionService transactionService) : Controller
    {
        private readonly ITransactionService _transactionService = transactionService;

        [HttpGet("", Name = "GetTransactions")]
        public async Task<IActionResult> GetTransactions()
        {

            try
            {
                var payload = await JwtDecoder.Decode(HttpContext);
                if (payload == null)
                {
                    return Unauthorized("Invalid or missing token.");
                }
                var googleId = payload.Subject;
                var transactions = await _transactionService.GetTransactionsAsync(googleId);
                Console.WriteLine(transactions);
                return Ok(transactions);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("account/{accountId}", Name = "GetTransactionsByAccountId")]
        public async Task<IActionResult> GetTransactionsByAccountId(int accountId)
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
                var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId, googleId);
                return Ok(transactions);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{transactionId}", Name = "GetTransactionsById")]
        public async Task<IActionResult> GetTransactionsById(int transactionId)
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
                var transactions = await _transactionService.GetTransactionsByIdAsync(transactionId, googleId);
                return Ok(transactions);
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