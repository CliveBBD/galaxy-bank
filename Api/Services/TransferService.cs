using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface ITransferService
    {
        Task<(int TransactionResult, string ReceiverName, string ReceiverEmail)> TransferAsync(TransferRequest transferRequest, string googleId);
    }

    public class TransferService : ITransferService
    {
        private readonly ITransferRepository _transferRepository;

        public TransferService(ITransferRepository transferRepository)
        {
            _transferRepository = transferRepository;
        }

        public async Task<(int TransactionResult, string ReceiverName, string ReceiverEmail)> TransferAsync(TransferRequest transferRequest, string googleId)
        {
            // Validate the request
            if (transferRequest == null || transferRequest.Amount <= 0)
            {
                throw new ArgumentException("Invalid transfer request.");
            }

            // Call the repository to perform the transfer operation
            return await _transferRepository.TransferAsync(transferRequest, googleId);
        }
    }
}