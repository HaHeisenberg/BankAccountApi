using BankAccountApi.DbContexts;
using BankAccountApi.Entities;
using BankProject.Shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace BankAccountApi.Services
{
    public interface IAccountRepository
    {
        Task<IEnumerable<AccountEntity>> GetAccountsAsync();
        Task<(IEnumerable<AccountEntity>, PaginationMetadata)> GetAccountAsync(
            string? searchQuery,
            int pageNumber,
            int pageSize);
        Task<AccountEntity?> GetAccountAsync(Guid accountId);

        Task<IEnumerable<AccountEntity>> GetAccountsByUserIdAsync(Guid userId);
        void CreateAccount(AccountEntity account);
        Task<bool> AccountExistsAsync(Guid accountId);
        Task<bool> SaveChangesAsync();
        void DeleteAccount(AccountEntity account);
        Task<int> AccountCountAsync();
    }
    public class AccountRepository : IAccountRepository
    {
        private AccountDbContext _context;

        public AccountRepository(AccountDbContext context)
        {
            _context = context ??
                       throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<AccountEntity>> GetAccountsAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<(IEnumerable<AccountEntity>, PaginationMetadata)> GetAccountAsync(
            string? searchQuery,
            int pageNumber,
            int pageSize)
        {
            var collection = _context.Accounts as IQueryable<AccountEntity>;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(
                    a => a.GetType()
                        .GetProperties()
                        .Any(
                            p => p.GetValue(a)
                                .ToString()
                                .Contains(searchQuery)));
            }

            var totalItemCount = await collection.CountAsync();

            var paginationMetadata = new PaginationMetadata(
                totalItemCount,
                pageSize,
                pageNumber);

            var collectionToReturn = await collection.
                Skip(pageSize * (pageSize - 1))
                .Take(pageSize)
                .ToListAsync();
            return (collectionToReturn, paginationMetadata);
        }

        public async Task<AccountEntity?> GetAccountAsync(Guid accountId)
        {
            return await _context.Accounts
                .Where(t => t.Id.Equals(accountId))
                .FirstOrDefaultAsync();
        }

        public void CreateAccount(AccountEntity account)
        {
            _context.Accounts.Add(account);
        }

        public async Task<bool> AccountExistsAsync(Guid accountId)
        {
            return await _context.Accounts.AnyAsync(
                t => t.Id.Equals(accountId));
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void DeleteAccount(AccountEntity account)
        {
            _context.Accounts.Remove(account);
        }

        public async Task<int> AccountCountAsync()
        {
            return await _context.Accounts.CountAsync();
        }

        public async Task<IEnumerable<AccountEntity>> GetAccountsByUserIdAsync(Guid userId)
        {
            return await _context.Accounts.Where(a => a.UserId.Equals(userId)).ToListAsync();
        }
    }
}
