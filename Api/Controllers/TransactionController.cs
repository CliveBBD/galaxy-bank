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
    [ApiController]
    public class TransactionsController(ITransactionService transactionService, IUserService userService) : Controller
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly IUserService _userService = userService;

        [HttpGet("", Name = "GetTransactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("User not authorized to view transactions", "You need to be logged in to get a list of transactions", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var transactions = await _transactionService.GetTransactionsAsync(requestingUser.GoogleID);
                return Ok(transactions);
            }
        }

        [HttpGet("disputable", Name = "GetDisputableTransactions")]
        public async Task<IActionResult> GetDisputableTransactions()
        {
            var requestingUser = HttpContext.GetCurrentUser();

            if (requestingUser != null && requestingUser.Role.Name == Constants.DisputeOfficerRoleName)
            {
                var transactions = await _transactionService.GetDisputableTransactionsAsync();
                return Ok(transactions);
            }
            else if (requestingUser != null && requestingUser.Role.Name != Constants.DisputeOfficerRoleName)
            {
                var transactions = await _transactionService.GetDisputableTransactionsAsync(requestingUser.UserID);
                return Ok(transactions);
            }
            else
            {
                return Unauthorized(new ErrorResponse("User is not authorized to view disputable transactions", "You need to be logged in to view disputable transactions", StatusCodes.Status401Unauthorized));
            }
        }

        [HttpGet("account/{accountNumber}", Name = "GetTransactionsByAccountNumber")]
        public async Task<IActionResult> GetTransactionsByAccountNumber(string accountNumber)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("User not authorized to view transactions by account number", "You need to be logged in to get a list of transactions by account number", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var googleId = requestingUser.GoogleID;
                var transactions = await _transactionService.GetTransactionsByAccountNumberAsync(accountNumber, googleId);
                return Ok(transactions);
            }
        }

        [HttpGet("{transactionId}", Name = "GetTransactionsById")]
        public async Task<IActionResult> GetTransactionsById(int transactionId)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("User not authorized to view transaction", "You need to be logged in to to view transactions", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var googleId = requestingUser.GoogleID;
                var transactions = await _transactionService.GetTransactionsByIdAsync(transactionId, googleId);
                return Ok(transactions);
            }
        }
    }
}