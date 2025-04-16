using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Api.Shared;

namespace Api.Controllers
{
    [Route("transaction-references")]
    public class TransactionReferencesController(ITransactionReferenceService transactionReferenceService) : Controller
    {
        private readonly ITransactionReferenceService _transactionReferenceService = transactionReferenceService;

        [HttpGet("{referenceId}", Name = "GetTransactionsByReferenceId")]
        public async Task<IActionResult> GetTransactionReferencesByAccountId(int referenceId)
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