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
        var lot = await _context.Lots.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lot is null)
        {
            return null;
        }

        var tagIds = await _context.LotTags
            .Where(lt => lt.LotId == id)
            .Select(lt => lt.TagId)
            .ToListAsync(cancellationToken);

        lot.LoadTags(tagIds);

        return lot;
    }

    public async Task AddAsync(Lot lot, CancellationToken cancellationToken = default)
    {
        await _context.Lots.AddAsync(lot, cancellationToken);
        if (lot.TagIds.Count > 0)
        {
            var lotTags = lot.TagIds.Select(tagId => new LotTag
            {
                LotId = lot.Id,
                TagId = tagId
            });
            await _context.LotTags.AddRangeAsync(lotTags, cancellationToken);
        }
    }

    public async Task UpdateAsync(Lot lot, CancellationToken cancellationToken = default)
    {
        _context.Lots.Update(lot);

        var existingLotTags = await _context.LotTags
            .Where(lt => lt.LotId == lot.Id)
            .ToListAsync(cancellationToken);

        var currentTagIds = lot.TagIds.ToHashSet();
        var existingTagIds = existingLotTags.Select(lt => lt.TagId).ToHashSet();

        var tagsToRemove = existingLotTags
            .Where(lt => !currentTagIds.Contains(lt.TagId))
            .ToList();

        var tagsToAdd = currentTagIds
            .Where(tagId => !existingTagIds.Contains(tagId))
            .Select(tagId => new LotTag
            {
                LotId = lot.Id,
                TagId = tagId
            })
            .ToList();

        if (tagsToRemove.Count > 0)
        {
            _context.LotTags.RemoveRange(tagsToRemove);
        }

        if (tagsToAdd.Count > 0)
        {
            await _context.LotTags.AddRangeAsync(tagsToAdd, cancellationToken);
        }
    }

    public void Delete(Lot lot)
    {
        _context.Lots.Remove(lot);
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
}