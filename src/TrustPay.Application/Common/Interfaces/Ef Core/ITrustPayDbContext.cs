namespace TrustPay.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TrustPay.Domain.Entities;

public interface ITrustPayDbContext
{
     DbSet<Category> Categories { get; }
     DbSet<Dispute> Disputes { get; }
     DbSet<Lot> Lots { get; }
     DbSet<Order> Orders { get; } 
     DbSet<SubCategory> SubCategories { get; }
     DbSet<Tag> Tags { get; }
     DbSet<User> Users { get; }
     DbSet<Wallet> Wallets { get; }
     DbSet<Review> Reviews { get; }
     DbSet<Transaction> Transactions { get; }
     DbSet<LotTag> LotTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}