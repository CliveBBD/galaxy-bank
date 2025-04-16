
using Api.Repositories;
using Api.Models;

namespace Api.Services
{
    public interface IAccountService
    {
        Task<string> CreateAccount(string accountTypeName);
        Task<IEnumerable<Account>> GetAccounts();
        Task<Account> GetAccountById(string id);
        Task<IEnumerable<Account>> GetAccountsByUserEmail(string email);
    }
    public class AccountService : IAccountService
    {
        private IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public async Task<string> CreateAccount(string accountTypeName)
        {
            return await _accountRepository.CreateAccountAsync(accountTypeName);
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await _accountRepository.GetAccountsAsync();
        }

        public async Task<Account> GetAccountById(string id)
        {
            return await _accountRepository.GetAccountByIdAsync(id);
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserEmail(string email)
        {
            return await _accountRepository.GetAccountsByUserEmailAsync(email);
        }
    }
}
