using Api.DTOs;
using Api.Repositories;
using System.ComponentModel.DataAnnotations;

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

            var validationContext = new ValidationContext(transferRequest);
            Validator.ValidateObject(transferRequest, validationContext, validateAllProperties: true);


            // Call the repository to perform the transfer operation
            return await _transferRepository.TransferAsync(transferRequest, googleId);
        }
    }
}