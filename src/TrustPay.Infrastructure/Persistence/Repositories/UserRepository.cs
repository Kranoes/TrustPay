namespace TrustPay.Infrastructure.Persistence.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Entities;

public class UserRepository : IUserRepository
{
    private readonly TrustPayDbContext _context;

    public UserRepository(TrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.UserEmail == email, cancellationToken);
    }

    public async Task<User?> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Wallet != null && u.Wallet.Id == walletId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(u => u.UserEmail == email, cancellationToken);
    }

    public async Task<bool> IsNickNameUnique(string nickName, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(u => u.UserName == nickName, cancellationToken);
    }
}