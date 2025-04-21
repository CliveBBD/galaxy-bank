using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _transactionService = new TransactionService(_mockTransactionRepository.Object);
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldReturnTransactions_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactions = new List<TransactionRequest>
            {
                new TransactionRequest
                {
                    TransactionID = 1,
                    TransactionReferenceID = 101,
                    Reference = "Ref1",
                    Amount = 100,
                    TransactionType = new TransactionType { TransactionTypeID = 1, Name = "Deposit" },
                    BalanceAfterTransaction = 1000,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "1234567890"
                },
                new TransactionRequest
                {
                    TransactionID = 2,
                    TransactionReferenceID = 102,
                    Reference = "Ref2",
                    Amount = 200,
                    TransactionType = new TransactionType { TransactionTypeID = 2, Name = "Withdrawal" },
                    BalanceAfterTransaction = 800,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "1234567890"
                }
            };

            _mockTransactionRepository
                .Setup(repo => repo.GetTransactionsAsync("test-google-id"))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetTransactionsAsync("test-google-id");

            // Xunit.Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(2, result.Count());
            Xunit.Assert.Contains(result, t => t.Reference == "Ref1");
            Xunit.Assert.Contains(result, t => t.Reference == "Ref2");
            _mockTransactionRepository.Verify(repo => repo.GetTransactionsAsync("test-google-id"), Times.Once);
        }

        [Fact]
        public async Task GetDisputableTransactionsAsync_ShouldReturnTransactions_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    TransactionID = 1,
                    TransactionReferenceID = 101,
                    Reference = "Ref1",
                    AccountID = 1,
                    Amount = 100,
                    TransactionType = new TransactionType { TransactionTypeID = 1, Name = "Deposit" },
                    BalanceAfterTransaction = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                new Transaction
                {
                    TransactionID = 2,
                    TransactionReferenceID = 102,
                    Reference = "Ref2",
                    AccountID = 2,
                    Amount = 200,
                    TransactionType = new TransactionType { TransactionTypeID = 2, Name = "Withdrawal" },
                    BalanceAfterTransaction = 800,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockTransactionRepository
                .Setup(repo => repo.GetDisputableTransactionsAsync(null))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetDisputableTransactionsAsync();

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(2, result.Count());
            Xunit.Assert.Contains(result, t => t.Reference == "Ref1");
            Xunit.Assert.Contains(result, t => t.Reference == "Ref2");
            _mockTransactionRepository.Verify(repo => repo.GetDisputableTransactionsAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsByAccountNumberAsync_ShouldReturnTransactions_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactions = new List<TransactionRequest>
            {
                new TransactionRequest
                {
                    TransactionID = 1,
                    TransactionReferenceID = 101,
                    Reference = "Ref1",
                    Amount = 100,
                    TransactionType = new TransactionType { TransactionTypeID = 1, Name = "Deposit" },
                    BalanceAfterTransaction = 1000,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "1234567890"
                }
            };

            _mockTransactionRepository
                .Setup(repo => repo.GetTransactionsByAccountNumberAsync("1234567890", "test-google-id"))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetTransactionsByAccountNumberAsync("1234567890", "test-google-id");

            // Xunit.Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Single(result);
            Xunit.Assert.Equal("1234567890", result.First().AccountNumber);
            _mockTransactionRepository.Verify(repo => repo.GetTransactionsByAccountNumberAsync("1234567890", "test-google-id"), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsByIdAsync_ShouldReturnTransaction_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactions = new List<TransactionRequest>
            {
                new TransactionRequest
                {
                    TransactionID = 1,
                    TransactionReferenceID = 101,
                    Reference = "Ref1",
                    Amount = 100,
                    TransactionType = new TransactionType { TransactionTypeID = 1, Name = "Deposit" },
                    BalanceAfterTransaction = 1000,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "1234567890"
                }
            };

            _mockTransactionRepository
                .Setup(repo => repo.GetTransactionsByIdAsync(1, "test-google-id"))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetTransactionsByIdAsync(1, "test-google-id");

            // Xunit.Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Single(result);
            Xunit.Assert.Equal(1, result.First().TransactionID);
            Xunit.Assert.Equal("Ref1", result.First().Reference);
            _mockTransactionRepository.Verify(repo => repo.GetTransactionsByIdAsync(1, "test-google-id"), Times.Once);
        }
    }
}