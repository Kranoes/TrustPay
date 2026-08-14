using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private TrustPayDbContext _context;
        public WalletRepository(TrustPayDbContext context)
        {
            _context = context;
        }
        public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }
        public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }
        public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)

        {
        await _context.Wallets.AddAsync(wallet,cancellationToken);
        }
        public void Update(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
        }
        public void Delete(Wallet wallet)
        {
            _context.Wallets.Remove(wallet);
        }



    }
}
