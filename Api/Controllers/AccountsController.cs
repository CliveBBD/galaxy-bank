using System.Text;
using Api.DTOs;
using Api.Helpers;
using Api.Models;
using Api.Services;
using Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly AccountMapper _accountMapper;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        public AccountsController(IAccountService accountService, AccountMapper accountMapper, IEmailService emailService, IUserService userService)
        {
            _accountService = accountService;
            _accountMapper = accountMapper;
            _emailService = emailService;
            _userService = userService;
        }

        [HttpPost("", Name = "CreateAccount")]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] AccountCreateRequest request)
        {
            var payload = await JwtDecoder.Decode(HttpContext);

            if (payload == null)
            {
                return Unauthorized(new ErrorResponse("You must be authenticated in order to create an account.", StatusCodes.Status401Unauthorized));
            }
            else
            {
                // Using this as a guard clause. If there is a payload continue with the normal flow. 
            }

            var userDto = new CreateUserDto(
                payload.Subject,
                payload.GivenName,
                payload.Email
            );

            var accountNumber = await _accountService.CreateAccount(request.AccountTypeName, userDto);

            if (accountNumber == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse(
                        "Account could not be created",
                        $"We could not create an account for {userDto.Username}. Please try again later.",
                        StatusCodes.Status500InternalServerError
                    )
                );
            }
            else
            {
                var account = await _accountService.GetAccountByAccountNumber(accountNumber);

                if (account == null)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new ErrorResponse(
                            "Created account could not be found",
                            $"We could not find the newly created account with the account number {accountNumber}. Please try again later.",
                            StatusCodes.Status500InternalServerError
                        )
                    );
                }
                else
                {
                    try
                    {
                        // We do not need to wait for the email to be sent
                        _ = _emailService.SendEmailAsync(userDto.Email, AccountCreationEmailTemplate.Subject, AccountCreationEmailTemplate.Message(userDto.Username, accountNumber));
                    }
                    catch
                    {
                        // Exceptions thrown by the emailing service shouldn't stop the normal flow of the endpoint
                    }

                    return StatusCode(StatusCodes.Status201Created, account);
                }
            }
        }

        [HttpGet("", Name = "GetAccounts")]
        public async Task<IActionResult> GetAccounts()
        {
            var requestingUser = HttpContext.GetCurrentUser();

            if (requestingUser == null)
            {
                return Unauthorized(new ErrorResponse("User is not authorized", "You must be authenticated in order to get a list of accounts.", StatusCodes.Status401Unauthorized));
            }
            else
            {
                var accounts = await _accountService.GetAccounts(requestingUser.GoogleID);

                if (accounts != null)
                {
                    var responseAccounts = await _accountMapper.ToAccountResponseList(accounts);
                    return Ok(responseAccounts);
                }
                else
                {
                    return NotFound(new ErrorResponse($"No accounts found for {requestingUser.Username}", StatusCodes.Status404NotFound));
                }
            }
        }

        [HttpGet("{accountNumber}", Name = "GetAccountByAccountNumber")]
        public async Task<IActionResult> GetAccountByAccountNumber(string accountNumber)
        {
            var account = await _accountService.GetAccountByAccountNumber(accountNumber);

            if (account == null)
            {
                return NotFound(new ErrorResponse(
                        "Account not found",
                        $"Account with account number: {accountNumber} not found.",
                        StatusCodes.Status404NotFound
                    )
                );
            }
            else
            {
                var response = await _accountMapper.ToAccountResponse(account);
                return Ok(response);
            }
        }

        [HttpGet("user/{email}", Name = "GetAccountsByUserEmail")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByUserEmail(string email)
        {
            var accounts = await _accountService.GetAccountsByUserEmail(email);

            if (!accounts.Any())
            {
                return NotFound(new ErrorResponse("No accounts found", $"No accounts found for {email}.", StatusCodes.Status404NotFound));
            }
            else
            {
                var responseAccounts = await _accountMapper.ToAccountResponseList(accounts);
                return Ok(responseAccounts);
            }
        }
    }
}