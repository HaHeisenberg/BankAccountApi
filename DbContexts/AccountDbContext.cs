using System.Text.Json;
using BankAccountApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using IdentityModel.Client; 


namespace BankAccountApi.DbContexts
{
    public class AccountDbContext : DbContext
    {
        public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

        public AccountDbContext(
            DbContextOptions<AccountDbContext> o) : base(o)
        {
        }

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    // get updated entries
        //    var updatedConcurrencyAwareEntries = ChangeTracker.Entries()
        //        .Where(e => e.State == EntityState.Modified)
        //        .OfType<IConcurrencyAware>();

        //    foreach (var entry in updatedConcurrencyAwareEntries)
        //    {
        //        entry.ConcurrencyStamp = Guid.NewGuid().ToString();
        //    }

        //    return base.SaveChangesAsync(cancellationToken);
        //}
    }
}