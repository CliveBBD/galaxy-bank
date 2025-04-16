
using Api.Repositories;
using Api.Models;

namespace Api.Services
{
    public interface IAccountService
    {
        Task<string> CreateAccount(string accountTypeName, CreateUserDto createUserDto);
        Task<IEnumerable<Account>> GetAccounts(int? userId = null);
        Task<Account> GetAccountByAccountNumber(string accountNumber);
        Task<IEnumerable<Account>> GetAccountsByUserEmail(string email);
    }
    public class AccountService : IAccountService
    {
        private IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public async Task<string> CreateAccount(string accountTypeName, CreateUserDto createUserDto)
        {
            return await _accountRepository.CreateAccountAsync(accountTypeName, createUserDto);
        }

        public async Task<IEnumerable<Account>> GetAccounts(int? userId)
        {
            return await _accountRepository.GetAccountsAsync(userId);
        }

        public async Task<Account> GetAccountByAccountNumber(string accountNumber)
        {
            return await _accountRepository.GetAccountByAccountNumberAsync(accountNumber);
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserEmail(string email)
        {
            return await _accountRepository.GetAccountsByUserEmailAsync(email);
        }
    }
}
