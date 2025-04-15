using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("transaction-references")]
    public class TransactionReferencesController(ITransactionReferenceService transactionReferenceService) : Controller
    {
        private readonly ITransactionReferenceService _transactionReferenceService = transactionReferenceService;

        [HttpGet("{referenceId}", Name = "GetTransactionReferencesByAccountId")]
        public async Task<IActionResult> GetTransactionReferencesByAccountId(int referenceId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string googleId = "google_id_b597e0d6d8e47ab0405e4627"; // Replace with actual Google ID from context.

            try
            {
                var transactionReferences = await _transactionReferenceService.GetTransactionsByReferenceAsync(googleId, referenceId);
                return Ok(transactionReferences);
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