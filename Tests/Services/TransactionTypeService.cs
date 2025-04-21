using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class TransactionTypeServiceTests
    {
        private readonly Mock<ITransactionTypeRepository> _mockTransactionTypeRepository;
        private readonly TransactionTypeService _transactionTypeService;

        public TransactionTypeServiceTests()
        {
            _mockTransactionTypeRepository = new Mock<ITransactionTypeRepository>();
            _transactionTypeService = new TransactionTypeService(_mockTransactionTypeRepository.Object);
        }

        [Fact]
        public async Task GetTransactionTypesAsync_ShouldReturnTransactionTypes_WhenRepositoryReturnsData()
        {
            // Arrange
            var transactionTypes = new List<TransactionType>
            {
                new TransactionType { TransactionTypeID = 1, Name = "Deposit" },
                new TransactionType { TransactionTypeID = 2, Name = "Withdrawal" }
            };

            _mockTransactionTypeRepository
                .Setup(repo => repo.GetTransactionTypesAsync())
                .ReturnsAsync(transactionTypes);

            // Act
            var result = await _transactionTypeService.GetTransactionTypesAsync();

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(2, result.Count());
            Xunit.Assert.Contains(result, t => t.Name == "Deposit");
            Xunit.Assert.Contains(result, t => t.Name == "Withdrawal");
            _mockTransactionTypeRepository.Verify(repo => repo.GetTransactionTypesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTransactionTypesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNoData()
        {
            // Arrange
            _mockTransactionTypeRepository
                .Setup(repo => repo.GetTransactionTypesAsync())
                .ReturnsAsync(new List<TransactionType>());

            // Act
            var result = await _transactionTypeService.GetTransactionTypesAsync();

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Empty(result);
            _mockTransactionTypeRepository.Verify(repo => repo.GetTransactionTypesAsync(), Times.Once);
        }
    }
}