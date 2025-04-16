using Api.DTOs;
using Api.Helpers;
using Api.Models;
using Api.Repositories;
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
        private readonly IAccountTypeRepository _accountTypeRepository;
        public AccountsController(IAccountService accountService, AccountMapper accountMapper, IEmailService emailService, IUserService userService, IAccountTypeRepository accountTypeRepository)
        {
            _accountService = accountService;
            _accountMapper = accountMapper;
            _emailService = emailService;
            _userService = userService;
            _accountTypeRepository = accountTypeRepository;
        }

        [HttpGet("account-types")]
        public async Task<IEnumerable<string>> GetAccountTypes()
        {
            return await _accountTypeRepository.GetAllAccountTypesAsync();
        }

        
        [HttpPost("account")]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] AccountCreateRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = $"Invalid account data." });
                }

                var accountId = await _accountService.CreateAccount(request.UserId, request.AccountTypeName);

                var account = await _accountService.GetAccountById(accountId);

                var user = await _userService.GetUserById(account.UserId);


                await _emailService.SendEmailAsync(user.Email, AccountCreationEmailTemplate.Subject, AccountCreationEmailTemplate.Message(user.Username, accountId));

                return Ok($"Successfully created an account with account number: {accountId}.");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAccounts();

                if (accounts == null)
                    return NotFound(new { message = $"No account was found." });

                var response = await _accountMapper.ToAccountResponseList(accounts);
                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            try
            {
                var account = await _accountService.GetAccountById(id);

                if (account == null)
                    return NotFound(new { message = $"Account with account number: {id} not found." });

                var response = await _accountMapper.ToAccountResponse(account);
                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

        }

        [HttpGet("user/{email}")]
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