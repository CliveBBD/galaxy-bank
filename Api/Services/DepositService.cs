using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IDepositService
    {
        Task<int> DepositAsync(DepositRequest depositRequest, string googleId);
    }

    public class DepositService : IDepositService
    {
        private readonly IDepositRepository _depositRepository;

        public DepositService(IDepositRepository depositRepository)
        {
            _depositRepository = depositRepository;
        }

        public async Task<int> DepositAsync(DepositRequest depositRequest, string googleId)
        {
            // Validate the request
            if (depositRequest == null || depositRequest.Amount <= 0)
            {
                throw new ArgumentException("Invalid deposit request.");
            }

            // Call the repository to perform the deposit operation
            return await _depositRepository.DepositAsync(depositRequest, googleId);
        }
    }
}