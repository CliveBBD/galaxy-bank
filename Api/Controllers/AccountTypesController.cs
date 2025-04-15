using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [ApiController]
    [Route("account_types")]
    public class AccountTypesController : ControllerBase
    {
        private readonly IAccountTypeRepository _accountTypeRepository;

        public AccountTypesController(IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeRepository = accountTypeRepository;
        }

        [HttpGet("{name}")]
        public async Task<AccountType> GetAccountTypeByName(string name)
        {
            var accountType = await _accountTypeRepository.GetAccountTypeByNameAsync(name);

            if (accountType == null)
                throw new ArgumentException($"Account type {name} does not exist, available account types are 'checking', 'savings' and 'credit_card'.");

            return await _accountTypeRepository.GetAccountTypeByNameAsync(name);
        }
    }
}