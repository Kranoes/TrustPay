namespace TrustPay.Infrastructure.Persistence.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
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
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByIdWithTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);
    }

    public async Task<User?> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await (from user in _context.Users
                      join wallet in _context.Wallets on user.Id equals wallet.UserId
                      where wallet.Id == walletId
                      select user)
                     .FirstOrDefaultAsync(cancellationToken);
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

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsNickNameUniqueAsync(string nickName, CancellationToken cancellationToken = default)
    {
        return !await _context.Users.AnyAsync(u => u.Name == nickName, cancellationToken);
    }
}