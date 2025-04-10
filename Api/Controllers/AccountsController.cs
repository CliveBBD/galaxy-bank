using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        private readonly DBConnection _connection;
        public AccountsController(IAccountService accountService, DBConnection connection)
        {
            _accountService = accountService;
            _connection = connection;
        }

        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] Account account)
        {
            if (account == null)
            {
                return BadRequest("Invalid account data.");
            }

            var accountId = await _accountService.CreateAccount(account);

            return Ok($"Successfully created an account with account number: {accountId}.");
          
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var account = await _accountService.GetAccountById(id);

            if (account == null)
                return NotFound(new { message = $"Account with ID {id} not found." });

            return Ok(account);
        }
    }
}
