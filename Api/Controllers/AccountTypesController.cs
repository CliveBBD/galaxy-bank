using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("account_types")]
    public class AccountTypesController : ControllerBase
    {
        private readonly IAccountTypeRepository _accountTypeRepository;

        public AccountTypesController(IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeRepository = accountTypeRepository;
        }

        [HttpGet("{name}", Name = "GetAccountTypeByName")]
        [ResponseCache(Duration = 82800)] // cache for 24 hours
        public async Task<IActionResult> GetAccountTypeByName(string name)
        {
            var accountType = await _accountTypeRepository.GetAccountTypeByNameAsync(name);

            if (accountType == null)
                return NotFound(new ErrorResponse("Account type not found", $"Account type {name} does not exist.", StatusCodes.Status404NotFound));
            else
                return Ok(accountType);
        }
    }
}