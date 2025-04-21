using Api.DTOs;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace Api.Tests.Services
{
    public class TransferServiceTests
    {
        private readonly Mock<ITransferRepository> _mockTransferRepository;
        private readonly TransferService _transferService;

        public TransferServiceTests()
        {
            _mockTransferRepository = new Mock<ITransferRepository>();
            _transferService = new TransferService(_mockTransferRepository.Object);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrowArgumentException_WhenTransferRequestIsNull()
        {
            // Arrange
            TransferRequest transferRequest = null;
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _transferService.TransferAsync(transferRequest, googleId));
        }

        [Fact]
        public async Task TransferAsync_ShouldThrowArgumentException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var transferRequest = new TransferRequest
            {
                FromAccountNumber = "1234567890",
                ToAccountNumber = "9876543210",
                Amount = 0
            };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _transferService.TransferAsync(transferRequest, googleId));
        }

        [Fact]
        public async Task TransferAsync_ShouldThrowArgumentException_WhenFromAccountNumberIsMissing()
        {
            // Arrange
            var transferRequest = new TransferRequest
            {
                FromAccountNumber = null,
                ToAccountNumber = "9876543210",
                Amount = 100
            };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ValidationException>(() => _transferService.TransferAsync(transferRequest, googleId));
        }

        [Fact]
        public async Task TransferAsync_ShouldThrowArgumentException_WhenToAccountNumberIsMissing()
        {
            // Arrange
            var transferRequest = new TransferRequest
            {
                FromAccountNumber = "1234567890",
                ToAccountNumber = null,
                Amount = 100
            };
            string googleId = "test-google-id";

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ValidationException>(() => _transferService.TransferAsync(transferRequest, googleId));
        }

        [Fact]
        public async Task TransferAsync_ShouldCallRepository_WhenRequestIsValid()
        {
            // Arrange
            var transferRequest = new TransferRequest
            {
                FromAccountNumber = "1234567890",
                ToAccountNumber = "9876543210",
                Amount = 100
            };
            string googleId = "test-google-id";

            _mockTransferRepository
                .Setup(repo => repo.TransferAsync(transferRequest, googleId))
                .ReturnsAsync((1, "John Doe", "john.doe@example.com")); // Simulate successful transfer

            // Act
            var result = await _transferService.TransferAsync(transferRequest, googleId);

            // Assert
            Xunit.Assert.Equal(1, result.TransactionResult);
            Xunit.Assert.Equal("John Doe", result.ReceiverName);
            Xunit.Assert.Equal("john.doe@example.com", result.ReceiverEmail);
            _mockTransferRepository.Verify(repo => repo.TransferAsync(transferRequest, googleId), Times.Once);
        }
    }
}