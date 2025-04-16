using Api.Repositories;
using Api.DTOs;
using Api.Models;

namespace Api.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionRequest>> GetTransactionsAsync(string googleId);
        Task<IEnumerable<Transaction>> GetDisputableTransactionsAsync(int? userId = null);
        Task<IEnumerable<TransactionRequest>> GetTransactionsByAccountNumberAsync(string accountNumber, string googleId);
        Task<IEnumerable<TransactionRequest>> GetTransactionsByIdAsync(int transactionId, string googleId);
    }

    public class TransactionService(ITransactionRepository transactionRepository) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

        public async Task<IEnumerable<Transaction>> GetDisputableTransactionsAsync(int? userId = null)
        {
            return await _transactionRepository.GetDisputableTransactionsAsync(userId);
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsAsync(string googleId)
        {
            return await _transactionRepository.GetTransactionsAsync(googleId);
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsByAccountNumberAsync(string accountNumber, string googleId)
        {
            return await _transactionRepository.GetTransactionsByAccountNumberAsync(accountNumber, googleId);
        }

        public async Task<IEnumerable<TransactionRequest>> GetTransactionsByIdAsync(int transactionId, string googleId)
        {
            return await _transactionRepository.GetTransactionsByIdAsync(transactionId, googleId);
        }
    }
}