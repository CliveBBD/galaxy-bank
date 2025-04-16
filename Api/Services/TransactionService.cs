using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId);
        Task<IEnumerable<Transaction>> GetDisputableTransactionsAsync(int? userId = null);
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId);
        Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId);
    }

    public class TransactionService(ITransactionRepository transactionRepository) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

        public async Task<IEnumerable<Transaction>> GetDisputableTransactionsAsync(int? userId = null)
        {
            return await _transactionRepository.GetDisputableTransactionsAsync(userId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string googleId)
        {
            return await _transactionRepository.GetTransactionsAsync(googleId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId, string googleId)
        {
            return await _transactionRepository.GetTransactionsByAccountIdAsync(accountId, googleId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByIdAsync(int transactionId, string googleId)
        {
            return await _transactionRepository.GetTransactionsByIdAsync(transactionId, googleId);
        }
    }
}