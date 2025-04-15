
using Api.Repositories;
using Api.Models;
using Api.DTOs;

namespace Api.Services
{
    public interface IAccountService
    {
        Task<int> CreateAccount(AccountCreateRequest accountDto);
        Task<IEnumerable<Account>> GetAccounts();
        Task<Account> GetAccountById(int id);
        Task<IEnumerable<Account>> GetAccountsByUserEmail(string email);
    }
    public class AccountService : IAccountService
    {
        private IAccountRepository _accountRepository;
        private readonly IAccountTypeRepository _accountTypeRepository;
        public AccountService(IAccountRepository accountRepository, IAccountTypeRepository accountTypeRepository)
        {
            _accountRepository = accountRepository;
            _accountTypeRepository = accountTypeRepository;
        }
        public async Task<int> CreateAccount(AccountCreateRequest accountDto)
        {
            int accountTypeId = accountDto.AccountType switch
            {

                AccountType.Checking => 4, // await _accountTypeRepository.GetAccountTypeIdByNameAsync("checking")
                AccountType.Savings => 5,
                AccountType.Credit_Card => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(accountDto.AccountType), "Invalid account type")
            };

            var accountType = _accountTypeRepository.GetAccountTypeByIdAsync(accountTypeId);

            if (accountType == null)
                throw new ArgumentException($"AccountType with ID {accountTypeId} does not exist.");

            if (accountDto.Balance < 0)
                throw new ArgumentException("Initial balance cannot be negative.");

            return await _accountRepository.CreateAccountAsync(accountDto);
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await _accountRepository.GetAccountsAsync();
        }

        public async Task<Account> GetAccountById(int id)
        {
            return await _accountRepository.GetAccountByIdAsync(id);
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserEmail(string email)
        {
            return await _accountRepository.GetAccountsByUserEmailAsync(email);
        }
    }
}
