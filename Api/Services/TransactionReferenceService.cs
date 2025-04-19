using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface ITransactionReferenceService
    {
        Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId);
        Task<TransactionReference?> GetTransactionReferenceById(int transactionReferenceId);
    }

    public class TransactionReferenceService(ITransactionReferenceRepository transactionReferenceRepository) : ITransactionReferenceService
    {
        private readonly ITransactionReferenceRepository _transactionReferenceRepository = transactionReferenceRepository;

        public async Task<TransactionReference?> GetTransactionReferenceById(int transactionReferenceId)
        {
            return await _transactionReferenceRepository.GetTransactionReferenceById(transactionReferenceId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByReferenceAsync(string googleId, int referenceId)
        {
            if (referenceId <= 0)
            {
                throw new ArgumentException("Reference ID must be greater than zero.", nameof(referenceId));
            }

            var transactions = await _transactionReferenceRepository.GetTransactionsByReferenceAsync(googleId, referenceId);
            return transactions;
        }
    }
}