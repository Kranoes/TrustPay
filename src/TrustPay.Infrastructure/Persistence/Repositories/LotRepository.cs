using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Repositories;

public class LotRepository : ILotRepository
{
    private readonly TrustPayDbContext _context;

    public LotRepository(TrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Lot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Lots
            .Include(l => l.Tags)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<Lot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Lots
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lot>> GetBySubCategoryIdAsync(Guid subCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Lots
            .AsNoTracking()
            .Where(l => l.SubCategoryId == subCategoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Lot lot, CancellationToken cancellationToken = default)
    {
        await _context.Lots.AddAsync(lot, cancellationToken);
    }

    public void Update(Lot lot)
    {
        _context.Lots.Update(lot);
    }

    public void Delete(Lot lot)
    {
        _context.Lots.Remove(lot);
    }
}