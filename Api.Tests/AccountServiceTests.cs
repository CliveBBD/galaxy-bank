using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _mockRepository;
        private readonly Mock<IAccountTypeRepository> _mockAccountTypeRepository;


        private readonly IAccountService _accountService;

        public AccountServiceTests()
        {
            _mockRepository = new Mock<IAccountRepository>();
            _mockAccountTypeRepository = new Mock<IAccountTypeRepository>();
            _accountService = new AccountService(
            _mockRepository.Object,
            _mockAccountTypeRepository.Object
        );
        }


        [Fact]
        public async Task GetAccounts_ShouldReturnAccounts()
        {
            var expectedAccounts = new List<Account>
            {
                new Account { AccountId = 1, UserId = 1, AccountTypeId = 2, Balance = 100, CreatedAt = DateTime.UtcNow },
                new Account { AccountId = 2, UserId = 2, AccountTypeId = 1, Balance = 200, CreatedAt = DateTime.UtcNow }
            };

            _mockRepository
                .Setup(r => r.GetAccountsAsync())
                .ReturnsAsync(expectedAccounts);

            var result = await _accountService.GetAccounts();

            Assert.NotNull(result);
            Assert.Equal(2, ((List<Account>)result).Count);
            _mockRepository.Verify(r => r.GetAccountsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccountById_ShouldReturnAccount()
        {
            var expectedAccount = new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountTypeId = 2,
                Balance = 150,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.GetAccountByIdAsync(1))
                .ReturnsAsync(expectedAccount);

            var result = await _accountService.GetAccountById(1);

            Assert.NotNull(result);
            Assert.Equal(expectedAccount.AccountId, result.AccountId);
            _mockRepository.Verify(r => r.GetAccountByIdAsync(1), Times.Once);
        }
    }
}
