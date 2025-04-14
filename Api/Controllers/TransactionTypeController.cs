using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("transaction-types")]
    public class TransactionTypeController(ITransactionTypeService transactionTypeService) : Controller
    {
        private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;

        [HttpGet("", Name = "GetTransactionType")]
        public async Task<IActionResult> GetTransactionType()
        {
            try
            {
                var transactionTypes = await _transactionTypeService.GetTransactionTypesAsync();
                Console.WriteLine(transactionTypes);
                return Ok(transactionTypes);
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