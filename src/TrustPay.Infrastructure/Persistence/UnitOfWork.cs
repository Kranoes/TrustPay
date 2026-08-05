using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Application.Common.Interfaces;

namespace TrustPay.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TrustPayDbContext _context;
        public UnitOfWork(TrustPayDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
