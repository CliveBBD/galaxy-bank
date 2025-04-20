using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("transaction-types")]
    public class TransactionTypeController(ITransactionTypeService transactionTypeService) : Controller
    {
        private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;

        [HttpGet("", Name = "GetTransactionType")]
        [ResponseCache(Duration = 82800)] // cache for 24 hours
        public async Task<IActionResult> GetTransactionType()
        {
            var transactionTypes = await _transactionTypeService.GetTransactionTypesAsync();
            return Ok(transactionTypes);
        }
    }
}