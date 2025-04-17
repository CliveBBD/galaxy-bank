using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _mockRepo;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mockRepo = new Mock<IAccountRepository>();
            _accountService = new AccountService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateAccount_Should_Call_Repository_And_Return_AccountNumber()
        {
            var accountTypeName = "checking";
            var createUserDto = new CreateUserDto("123", "tester", "test@example.com");
            var expectedAccountNumber = "123456789";

            _mockRepo.Setup(r => r.CreateAccountAsync(accountTypeName, createUserDto))
                .ReturnsAsync(expectedAccountNumber);

            var result = await _accountService.CreateAccount(accountTypeName, createUserDto);

            Assert.Equal(expectedAccountNumber, result);
            _mockRepo.Verify(r => r.CreateAccountAsync(accountTypeName, createUserDto), Times.Once);
        }

        [Fact]
        public async Task GetAccounts_Should_Call_Repository_And_Return_Accounts()
        {
            var googleId = "abc123";
            var expectedAccounts = new List<Account>
            {
                new Account
                {
                    AccountId = 1,
                    UserId = 1,
                    AccountTypeId = 1,
                    Balance = 5000,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "111",
                },
                new Account
                {
                    AccountId = 2,
                    UserId = 1,
                    AccountTypeId = 2,
                    Balance = 3000,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "222",
                }
            };

            _mockRepo.Setup(r => r.GetAccountsAsync(googleId))
                .ReturnsAsync(expectedAccounts);

            var result = await _accountService.GetAccounts(googleId);

            Assert.Equal(expectedAccounts.Count, ((List<Account>)result).Count);
            _mockRepo.Verify(r => r.GetAccountsAsync(googleId), Times.Once);
        }

        [Fact]
        public async Task GetAccountByAccountNumber_Should_Return_Account()
        {
            var accountNumber = "123456";
            var expectedAccount = new Account
            {
                AccountId = 1,
                UserId = 2,
                AccountTypeId = 1,
                Balance = 7000,
                CreatedAt = DateTime.UtcNow,
                AccountNumber = accountNumber,
            };

            _mockRepo.Setup(r => r.GetAccountByAccountNumberAsync(accountNumber))
                .ReturnsAsync(expectedAccount);

            var result = await _accountService.GetAccountByAccountNumber(accountNumber);

            Assert.NotNull(result);
            Assert.Equal(accountNumber, result.AccountNumber);
            _mockRepo.Verify(r => r.GetAccountByAccountNumberAsync(accountNumber), Times.Once);
        }

        [Fact]
        public async Task GetAccountsByUserEmail_Should_Return_Accounts()
        {
            var email = "user@example.com";
            var expectedAccounts = new List<Account>
            {
                new Account
                {
                    AccountId = 3,
                    UserId = 2,
                    AccountTypeId = 2,
                    Balance = 1500,
                    CreatedAt = DateTime.UtcNow,
                    AccountNumber = "999",
                }
            };

            _mockRepo.Setup(r => r.GetAccountsByUserEmailAsync(email))
                .ReturnsAsync(expectedAccounts);

            var result = await _accountService.GetAccountsByUserEmail(email);

            Assert.Single(result);
            _mockRepo.Verify(r => r.GetAccountsByUserEmailAsync(email), Times.Once);
        }
    }
}
