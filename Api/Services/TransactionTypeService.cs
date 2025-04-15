using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface ITransactionTypeService
    {
        Task<IEnumerable<TransactionType>> GetTransactionTypesAsync();
    }

    public class TransactionTypeService(ITransactionTypeRepository transactionTypeRepository) : ITransactionTypeService
    {
        private readonly ITransactionTypeRepository _transactionTypeRepository = transactionTypeRepository;
        public async Task<IEnumerable<TransactionType>> GetTransactionTypesAsync()
        {
            return await _transactionTypeRepository.GetTransactionTypesAsync();
        }
    }
}