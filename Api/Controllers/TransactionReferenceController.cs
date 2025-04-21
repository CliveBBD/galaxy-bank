using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Api.DTOs;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("transaction-references")]
    public class TransactionReferencesController(ITransactionReferenceService transactionReferenceService) : Controller
    {
        private readonly ITransactionReferenceService _transactionReferenceService = transactionReferenceService;

        [HttpGet("{referenceId}", Name = "GetTransactionsByReferenceId")]
        public async Task<IActionResult> GetTransactionReferencesByAccountId(int referenceId)
        {
            var requestingUser = HttpContext.GetCurrentUser();
            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("User not authorized to view transactions by reference id", "You need to be authorized to view transactions by transaction id", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var googleId = requestingUser.GoogleID;
                var transactionReferences = await _transactionReferenceService.GetTransactionsByReferenceAsync(googleId, referenceId);
                return Ok(transactionReferences);
            }
        }
    }
}