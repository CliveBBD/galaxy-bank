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
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = $"Invalid account data." });
                }

                var payload = await JwtDecoder.Decode(HttpContext);

                var userDto = new CreateUserDto(
                    payload.Subject,
                    payload.GivenName,
                    payload.Email
                );

                var accountNumber = await _accountService.CreateAccount(request.AccountTypeName, userDto);

                var account = await _accountService.GetAccountByAccountNumber(accountNumber);

                var user = await _userService.GetUserByIdAsync(account.UserId);


                await _emailService.SendEmailAsync(user.Email, AccountCreationEmailTemplate.Subject, AccountCreationEmailTemplate.Message(user.Username, accountNumber));

                return Ok($"Successfully created an account with account number: {accountNumber}.");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet("", Name = "GetAccounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var requestingUser = await _userService.GetCurrentUser(HttpContext);


                var googleId = requestingUser.GoogleID;
                var accounts = await _accountService.GetAccounts(googleId);

                if (accounts != null)
                {
                    var response = await _accountMapper.ToAccountResponseList(accounts);
                    return Ok(response);
                }
                else
                {
                    return NotFound(new ErrorResponse($"No accounts found"));
                }


            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet("{accountNumber}", Name = "GetAccountByAccountNumber")]
        public async Task<IActionResult> GetAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var account = await _accountService.GetAccountByAccountNumber(accountNumber);

                if (account == null)
                    return NotFound(new { message = $"Account with account number: {accountNumber} not found." });

                var response = await _accountMapper.ToAccountResponse(account);
                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet("user/{email}", Name = "GetAccountsByUserEmail")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByUserEmail(string email)
        {

            try
            {
                var accounts = await _accountService.GetAccountsByUserEmail(email);

                if (!accounts.Any())
                    return NotFound(new { message = $"No accounts found for {email}." });

                var response = await _accountMapper.ToAccountResponseList(accounts);
                return Ok(response);


            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}