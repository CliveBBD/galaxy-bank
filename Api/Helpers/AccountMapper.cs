using Api.Models;
using Api.Repositories;

namespace Api.Helpers
{
    public class AccountMapper
    {
        private readonly IAccountTypeRepository _accountTypeRepository;


        public AccountMapper()
        {

        }
        public AccountMapper(IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeRepository = accountTypeRepository;
        }

        public async Task<AccountResponse> ToAccountResponse(Account account)
        {
            var accountType = await _accountTypeRepository.GetAccountTypeByIdAsync(account.AccountTypeId);

            if (accountType == null)
                throw new Exception($"Account type with ID {account.AccountTypeId} not found.");

            return new AccountResponse
            {
                AccountId = account.AccountId,
                UserId = account.UserId,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                AccountType = accountType
            };
        }

        public async Task<IEnumerable<AccountResponse>> ToAccountResponseList(IEnumerable<Account> accounts)
        {

            var accountResponseTasks = accounts.Select(async account =>
                {
                    var accountType = await _accountTypeRepository.GetAccountTypeByIdAsync(account.AccountTypeId);

                    return new AccountResponse
                    {
                        AccountId = account.AccountId,
                        UserId = account.UserId,
                        Balance = account.Balance,
                        CreatedAt = account.CreatedAt,
                        AccountType = accountType!
                    };
                });

            var accountResponses = await Task.WhenAll(accountResponseTasks);
            return accountResponses;

        }
    }
}
