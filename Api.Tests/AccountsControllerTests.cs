using Api.Controllers;
using Api.DTOs;
using Api.Models;
using Api.Services;
using Api.Repositories;
using Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers
{
    public class AccountsControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly AccountsController _controller;


        public AccountsControllerTests()
        {
            _controller = new AccountsController(
                _accountServiceMock.Object,
                new AccountMapper(),
                _emailServiceMock.Object,
                _userServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateAccount_ReturnsOk_WhenSuccessful()
        {
            var request = new AccountCreateRequest { UserId = 1, AccountType = new AccountType(), Balance = 1000 };
            var account = new Account { AccountId = 10, UserId = 1, AccountTypeId = 2, Balance = 1000, CreatedAt = DateTime.UtcNow };
            var user = new User { UserID = 1, GoogleID = "g123", Username = "test", Email = "test@example.com", Role = new Role { RoleID = 1, Name = "Customer" } };

            _accountServiceMock.Setup(x => x.CreateAccount(request)).ReturnsAsync(10);
            _accountServiceMock.Setup(x => x.GetAccountById(10)).ReturnsAsync(account);
            _userServiceMock.Setup(x => x.GetUserById(1)).ReturnsAsync(user);
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await _controller.CreateAccount(request);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Successfully created an account with account number: 10.", okResult.Value);
        }


        [Fact]
        public async Task GetAccount_ReturnsNotFound_WhenAccountMissing()
        {
            _accountServiceMock.Setup(x => x.GetAccountById(100)).ReturnsAsync((Account)null!);

            var result = await _controller.GetAccount(100);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value!.ToString());
        }

        [Fact]
        public async Task CreateAccount_ReturnsBadRequest_WhenRequestIsNull()
        {
            var result = await _controller.CreateAccount(null!);

            var badResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Invalid account data", badResult.Value!.ToString());
        }
    }
}
