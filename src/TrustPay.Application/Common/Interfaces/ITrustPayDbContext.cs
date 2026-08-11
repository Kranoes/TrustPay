namespace TrustPay.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TrustPay.Domain.Entities;

public interface ITrustPayDbContext
{
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<Dispute> Disputes { get; }
    DbSet<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}