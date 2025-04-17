using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.Shared;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("transactions")]
    public class TransactionsController(ITransactionService transactionService, IUserService userService) : Controller
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly IUserService _userService = userService;

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

        [HttpGet("disputable", Name = "GetDisputableTransactions")]
        public async Task<IActionResult> GetDisputableTransactions()
        {
            try
            {

                var requestingUser = await _userService.GetCurrentUser(HttpContext);

                if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                {
                    var transactions = await _transactionService.GetDisputableTransactionsAsync();
                    return Ok(transactions);
                }
                else if (requestingUser != null && requestingUser.Role.Name != Constants.AdminRoleName)
                {
                    var transactions = await _transactionService.GetDisputableTransactionsAsync(requestingUser.UserID);
                    return Ok(transactions);
                }
                else
                {
                    return Unauthorized(new ErrorResponse("You are not authorized to use this feature. Please log in and try again"));
                }

            }
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse(e.Message)
                );
            }
        }


        [HttpGet("account/{accountNumber}", Name = "GetTransactionsByAccountNumber")]
        public async Task<IActionResult> GetTransactionsByAccountNumber(string accountNumber)
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
                var transactions = await _transactionService.GetTransactionsByAccountNumberAsync(accountNumber, googleId);
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