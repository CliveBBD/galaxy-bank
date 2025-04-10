using Api.Interfaces;
using Api.Models;

namespace Api.Services
{
    public interface IAccountService
    {
        Task<int> CreateAccount(Account account);
        Task<Account> GetAccountById(int id);
    }
    public class AccountService: IAccountService
    {
        private IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public Task<int> CreateAccount(Account account)
        {
            
            return _accountRepository.CreateAccountAsync(account);
        }

        public Task<Account> GetAccountById(int id)
        {
            return _accountRepository.GetAccountByIdAsync(id);
        }
    }
}
