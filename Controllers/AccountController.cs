using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using BankAccountApi.Entities;
using BankAccountApi.Models;
using BankAccountApi.Services;
using BankProject.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using IdentityModel;

namespace BankAccountApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private IAccountRepository _accountRepository;
        private IMapper _mapper;
        private readonly int maxPageSize = 20;

        public AccountController(
            IAccountRepository accountRepository,
            IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts(
            string? searchQuery,
            int pageNumber,
            int pageSize)
        {
            if (pageSize > maxPageSize)
            {
                pageSize = maxPageSize;
            }

            var (accountEntities
                    , paginationMetadata) =
                await _accountRepository.GetAccountAsync(
                    searchQuery,
                    pageNumber,
                    pageSize);

            Response.Headers.Add(
                "X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            return Ok(
                _mapper.Map<IEnumerable<AccountDto>>(accountEntities));
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            return Ok(
                _mapper.Map<IEnumerable<AccountDto>>(
                    await _accountRepository.GetAccountsAsync()));
        }
        //[Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByUserId()
        {
            var x = User;

            var sub = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var userId = Guid.Parse(sub.Substring(6));
            if(userId == null)
            {
                throw new Exception("User ID missing from Token");
            }
            var accounts = _mapper.Map<IEnumerable<AccountDto>>(await _accountRepository.GetAccountsByUserIdAsync(userId));
            var response = Ok(accounts);

            return response;
        }

        [HttpGet(
            "{accountId}",
            Name = "GetAccount")]
        public async Task<ActionResult> GetAccount(
            Guid accountId)
        {
            var accountEntity = await _accountRepository.GetAccountAsync(accountId);

            if (accountEntity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AccountDto>(accountEntity));
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount(AccountForCreationDto account)
        {
            var finalAccount = _mapper.Map<AccountEntity>(account);

            finalAccount.Number = await GenerateAccountNumber(finalAccount.Type);

            _accountRepository.CreateAccount(finalAccount);
            await _accountRepository.SaveChangesAsync();

            var createdAccountToReturn = _mapper.Map<AccountDto>(finalAccount);

            return CreatedAtRoute(
                "GetAccount",
                new { accountId = finalAccount.Id },
                createdAccountToReturn);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> Transfer(TransactionForProcessingDto transaction)
        {
            if (!await _accountRepository.AccountExistsAsync(transaction.SenderAccountId)
                || !await _accountRepository.AccountExistsAsync(transaction.ReceiverAccountId))
            {
                return NotFound();
            }

            var senderAccount = await _accountRepository.GetAccountAsync(transaction.SenderAccountId);
            var senderAccountToPatch = _mapper.Map<AccountForUpdateDto>(senderAccount);

            var receiverAccount = await _accountRepository.GetAccountAsync(transaction.ReceiverAccountId);
            var receiverAccountToPatch = _mapper.Map<AccountForUpdateDto>(receiverAccount);

            var senderNewBalance = senderAccountToPatch.Balance - transaction.Amount;
            var receiverNewBalance = receiverAccountToPatch.Balance + transaction.Amount;

            var senderPatch = new JsonPatchDocument<AccountForUpdateDto>();
            senderPatch.Replace(x => x.Balance, senderNewBalance);
            senderPatch.ApplyTo(senderAccountToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!TryValidateModel(senderAccountToPatch))
            {
                return BadRequest(ModelState);
            }

            var receiverPatch = new JsonPatchDocument<AccountForUpdateDto>();
            receiverPatch.Replace(x => x.Balance, receiverNewBalance);
            receiverPatch.ApplyTo(receiverAccountToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!TryValidateModel(receiverAccountToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(senderAccountToPatch, senderAccount);
            _mapper.Map(receiverAccountToPatch, receiverAccount);

            await _accountRepository.SaveChangesAsync();

            return Ok();
        }

        /*
        [HttpPut("{accountId}")]
        public async Task<ActionResult> UpdateAccount(
            Guid accountId,
            AccountForUpdateDto account)
        {
            if (!await _accountRepository.AccountExistsAsync(accountId))
            {
                return NotFound();
            }

            var accountEntity = await _accountRepository.GetAccountAsync(accountId);
            if (accountEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(
                account,
                accountEntity);

            await _accountRepository.SaveChangesAsync();
            return NoContent();
        }
        */
        /*
        [HttpPatch]
        public async Task<ActionResult> PartiallyUpdateAccount(
            Guid accountId,
            JsonPatchDocument<AccountForUpdateDto> patchDocument)
        {
            if (!await _accountRepository.AccountExistsAsync(accountId))
            {
                return NotFound();
            }

            var accountEntity = await _accountRepository.GetAccountAsync(accountId);

            if (accountEntity == null)
            {
                return NotFound();
            }

            var accountToPatch = _mapper.Map<AccountForUpdateDto>(accountEntity);

            patchDocument.ApplyTo(accountToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!TryValidateModel(accountToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(
                accountToPatch,
                accountEntity);

            await _accountRepository.SaveChangesAsync();

            return NoContent();
        }*/

        [HttpDelete]
        public async Task<ActionResult> DeleteAccount(Guid accountId)
        {
            if (!await _accountRepository.AccountExistsAsync(accountId))
            {
                return NotFound();
            }

            var accountEntity = await _accountRepository.GetAccountAsync(accountId);

            if (accountEntity == null)
            {
                return NotFound();
            }

            _accountRepository.DeleteAccount(accountEntity);
            await _accountRepository.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> GenerateAccountNumber(string type)
        {
            var rand = new Random();
            var accountTypeId = type == "Investment" ? "3333" : type == "Saving" ? "2222" : "1111";
            var accountNumber = 
                    "80" +
                    accountTypeId +
                    "0000" +
                    rand.Next(
                        1111,
                        9999) +
                    rand.Next(
                        1111111,
                        1111999) +
                    (10000 +
                    await _accountRepository.AccountCountAsync());

            return accountNumber;
        }
    }
}
