using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId);
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId);
        Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId);
    }

    public class TransactionService(ITransactionRepository roleRepository) : ITransactionService
    {
        private readonly ITransactionRepository _roleRepository = roleRepository;
        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId)
        {
            return await _roleRepository.GetTransactionsAsync(googleId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId)
        {
            return await _roleRepository.GetTransactionsByAccountIdAsync(accountId, googleId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId)
        {
            return await _roleRepository.GetTransactionsByIdAsync(transactionId, googleId);
        }
    }
}