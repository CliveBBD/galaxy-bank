using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class TransactionReferenceServiceTests
    {
        private readonly Mock<ITransactionReferenceRepository> _mockTransactionReferenceRepository;
        private readonly TransactionReferenceService _transactionReferenceService;

        public TransactionReferenceServiceTests()
        {
            _mockTransactionReferenceRepository = new Mock<ITransactionReferenceRepository>();
            _transactionReferenceService = new TransactionReferenceService(_mockTransactionReferenceRepository.Object);
        }

        [Fact]
        public async Task GetTransactionReferenceById_ShouldReturnTransactionReference_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactionReference = new TransactionReference { TransactionReferenceID = 1 };
            _mockTransactionReferenceRepository
                .Setup(repo => repo.GetTransactionReferenceById(1))
                .ReturnsAsync(transactionReference);

            // Act
            var result = await _transactionReferenceService.GetTransactionReferenceById(1);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(1, result.TransactionReferenceID);
            _mockTransactionReferenceRepository.Verify(repo => repo.GetTransactionReferenceById(1), Times.Once);
        }

        [Fact]
        public async Task GetTransactionReferenceById_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _mockTransactionReferenceRepository
                .Setup(repo => repo.GetTransactionReferenceById(1))
                .ReturnsAsync((TransactionReference?)null);

            // Act
            var result = await _transactionReferenceService.GetTransactionReferenceById(1);

            // Xunit.Assert
            Xunit.Assert.Null(result);
            _mockTransactionReferenceRepository.Verify(repo => repo.GetTransactionReferenceById(1), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsByReferenceAsync_ShouldThrowArgumentException_WhenReferenceIdIsZeroOrNegative()
        {
            // Arrange
            var invalidReferenceId = 0;

            // Act & Xunit.Assert
            var exception = await Xunit.Assert.ThrowsAsync<ArgumentException>(() =>
                _transactionReferenceService.GetTransactionsByReferenceAsync("test-google-id", invalidReferenceId));

            Xunit.Assert.Equal("Reference ID must be greater than zero. (Parameter 'referenceId')", exception.Message);
        }

        [Fact]
        public async Task GetTransactionsByReferenceAsync_ShouldReturnTransactionsWithAllProperties_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    TransactionID = 1,
                    TransactionReferenceID = 101,
                    Reference = "Ref1",
                    AccountID = 1001,
                    Amount = 500,
                    TransactionType = new TransactionType{ TransactionTypeID = 1, Name = "deposit" },
                    BalanceAfterTransaction = 1500,
                    CreatedAt = DateTime.UtcNow
                },
                new Transaction
                {
                    TransactionID = 2,
                    TransactionReferenceID = 101,
                    Reference = "Ref2",
                    AccountID = 1002,
                    Amount = 300,
                    TransactionType = new TransactionType{ TransactionTypeID = 2, Name = "withdrawal" },
                    BalanceAfterTransaction = 700,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockTransactionReferenceRepository
                .Setup(repo => repo.GetTransactionsByReferenceAsync("test-google-id", 101))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionReferenceService.GetTransactionsByReferenceAsync("test-google-id", 101);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(2, result.Count());
            Xunit.Assert.All(result, t =>
            {
                Xunit.Assert.True(t.TransactionID > 0);
                Xunit.Assert.True(t.TransactionReferenceID > 0);
                Xunit.Assert.False(string.IsNullOrWhiteSpace(t.Reference));
                Xunit.Assert.True(t.AccountID > 0);
                Xunit.Assert.True(t.Amount > 0);
                Xunit.Assert.NotNull(t.TransactionType);
                Xunit.Assert.True(t.BalanceAfterTransaction >= 0);
                Xunit.Assert.True(t.CreatedAt <= DateTime.UtcNow);
            });
            _mockTransactionReferenceRepository.Verify(repo => repo.GetTransactionsByReferenceAsync("test-google-id", 101), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsByReferenceAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNoData()
        {
            // Arrange
            _mockTransactionReferenceRepository
                .Setup(repo => repo.GetTransactionsByReferenceAsync("test-google-id", 101))
                .ReturnsAsync(new List<Transaction>());

            // Act
            var result = await _transactionReferenceService.GetTransactionsByReferenceAsync("test-google-id", 101);

            // Xunit.Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Empty(result);
            _mockTransactionReferenceRepository.Verify(repo => repo.GetTransactionsByReferenceAsync("test-google-id", 101), Times.Once);
        }
    }
}