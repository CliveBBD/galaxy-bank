using Api.DTOs;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class WithdrawServiceTests
    {
        private readonly Mock<IWithdrawRepository> _mockWithdrawRepository;
        private readonly WithdrawService _withdrawService;

        public WithdrawServiceTests()
        {
            _mockWithdrawRepository = new Mock<IWithdrawRepository>();
            _withdrawService = new WithdrawService(_mockWithdrawRepository.Object);
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowArgumentException_WhenWithdrawRequestIsNull()
        {
            // Arrange
            WithdrawRequest withdrawRequest = null;
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _withdrawService.WithdrawAsync(withdrawRequest, googleId));
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowArgumentException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var withdrawRequest = new WithdrawRequest { Amount = 0, AccountNumber = "1234567890" };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _withdrawService.WithdrawAsync(withdrawRequest, googleId));
        }

        [Fact]
        public async Task WithdrawAsync_ShouldThrowArgumentException_WhenAccountNumberIsNull()
        {
            // Arrange
            var withdrawRequest = new WithdrawRequest { Amount = 100, AccountNumber = null };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _withdrawService.WithdrawAsync(withdrawRequest, googleId));
        }

        [Fact]
        public async Task WithdrawAsync_ShouldCallRepository_WhenRequestIsValid()
        {
            // Arrange
            var withdrawRequest = new WithdrawRequest { Amount = 100, AccountNumber = "1234567890" };
            string googleId = "test-google-id";

            _mockWithdrawRepository
                .Setup(repo => repo.WithdrawAsync(withdrawRequest, googleId))
                .ReturnsAsync(1); // Simulate successful withdrawal

            // Act
            var result = await _withdrawService.WithdrawAsync(withdrawRequest, googleId);

            // Assert
            Xunit.Assert.Equal(1, result);
            _mockWithdrawRepository.Verify(repo => repo.WithdrawAsync(withdrawRequest, googleId), Times.Once);
        }
    }
}