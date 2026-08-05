using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Repositories;

public class SubCategoryRepository : ISubCategoryRepository
{
    private readonly TrustPayDbContext _context;

    public SubCategoryRepository(TrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<SubCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SubCategories
            .Include(sc => sc.Tags)
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);
    }

    public async Task<List<SubCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.SubCategories
            .AsNoTracking()
            .Where(sc => sc.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SubCategory subCategory, CancellationToken cancellationToken = default)
    {
        await _context.SubCategories.AddAsync(subCategory, cancellationToken);
    }

    public void Update(SubCategory subCategory)
    {
        _context.SubCategories.Update(subCategory);
    }

    public void Delete(SubCategory subCategory)
    {
        _context.SubCategories.Remove(subCategory);
    }
}