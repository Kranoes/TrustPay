using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Interfaces;
using TrustPay.Infrastructure.Persistence; 

namespace TrustPay.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly TrustPayDbContext _context;

        public TagRepository(TrustPayDbContext context)
        {
            _context = context;
        }

        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.SubCategories)
                .Include(t => t.Lots)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Tag?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.NormalizedName == normalizedName, cancellationToken);
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .AnyAsync(t => t.NormalizedName == normalizedName, cancellationToken);
        }

        public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            await _context.Tags.AddAsync(tag, cancellationToken);
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _context.Tags.Update(tag);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _context.Tags.Remove(tag);
            await Task.CompletedTask;
        }
    }
}