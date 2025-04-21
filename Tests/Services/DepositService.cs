using Api.DTOs;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class DepositServiceTests
    {
        private readonly Mock<IDepositRepository> _mockDepositRepository;
        private readonly DepositService _depositService;

        public DepositServiceTests()
        {
            _mockDepositRepository = new Mock<IDepositRepository>();
            _depositService = new DepositService(_mockDepositRepository.Object);
        }

        [Fact]
        public async Task DepositAsync_ShouldThrowArgumentException_WhenDepositRequestIsNull()
        {
            // Arrange
            DepositRequest depositRequest = null;
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _depositService.DepositAsync(depositRequest, googleId));
        }

        [Fact]
        public async Task DepositAsync_ShouldThrowArgumentException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var depositRequest = new DepositRequest { Amount = 0, AccountNumber = "1234567890" };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _depositService.DepositAsync(depositRequest, googleId));
        }

        [Fact]
        public async Task DepositAsync_ShouldThrowArgumentException_WhenAccountNumberIsNull()
        {
            // Arrange
            var depositRequest = new DepositRequest { Amount = 100, AccountNumber = null };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _depositService.DepositAsync(depositRequest, googleId));
        }

        [Fact]
        public async Task DepositAsync_ShouldCallRepository_WhenRequestIsValid()
        {
            // Arrange
            var depositRequest = new DepositRequest { Amount = 100, AccountNumber = "1234567890" };
            string googleId = "test-google-id";

            _mockDepositRepository
                .Setup(repo => repo.DepositAsync(depositRequest, googleId))
                .ReturnsAsync(1); // Simulate successful deposit

            // Act
            var result = await _depositService.DepositAsync(depositRequest, googleId);

            // Assert
            Xunit.Assert.Equal(1, result);
            _mockDepositRepository.Verify(repo => repo.DepositAsync(depositRequest, googleId), Times.Once);
        }
    }
}