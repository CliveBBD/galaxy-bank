using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public interface IWithdrawService
    {
        Task<int> WithdrawAsync(WithdrawRequest withdrawRequest, string googleId);
    }

    public class WithdrawService : IWithdrawService
    {
        private readonly IWithdrawRepository _withdrawRepository;

        public WithdrawService(IWithdrawRepository withdrawRepository)
        {
            _withdrawRepository = withdrawRepository;
        }

        public async Task<int> WithdrawAsync(WithdrawRequest withdrawRequest, string googleId)
        {
            // Validate the request
            if (withdrawRequest == null || withdrawRequest.Amount <= 0)
            {
                throw new ArgumentException("Invalid withdraw request.");
            }

            // Call the repository to perform the withdraw operation
            return await _withdrawRepository.WithdrawAsync(withdrawRequest, googleId);
        }
    }
}