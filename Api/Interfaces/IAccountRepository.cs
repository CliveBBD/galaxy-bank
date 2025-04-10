using Api.Models;

namespace Api.Interfaces
{
    public interface IAccountRepository
    {
        Task<int> CreateAccountAsync(Account account); 
        Task<Account> GetAccountByIdAsync(int id);
    }
}
