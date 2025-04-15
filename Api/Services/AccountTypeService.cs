using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IAccountTypeService
    {
        Task<AccountType> GetAccountTypeByName(string name);
    }
    public class AccountTypeService : IAccountTypeService
    {
        private readonly IAccountTypeRepository _accountTypeRepository;
        public AccountTypeService(IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeRepository = accountTypeRepository;
        }
        public async Task<AccountType> GetAccountTypeByName(string name)
        {

            return await _accountTypeRepository.GetAccountTypeByNameAsync(name);
        }
    }
}
