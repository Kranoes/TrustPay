using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
namespace TrustPay.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly TrustPayDbContext _context;
        public CategoryRepository(TrustPayDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }
        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        public async Task<Guid?> GetIdByTitle(string title, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                 .Where(c => c.Title == title)
                 .Select(c => (Guid?)c.Id)
                 .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);

        }
        public async Task<List<Category>> SearchAsync(string? title, string? description, CategoryType? type, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.AsQueryable().AsNoTracking();
            if (title is not null&&string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(c=>EF.Functions.ILike(c.Title, $"%{title}%"));
            }
            if (description is not null&&string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(c=>EF.Functions.ILike(c.Description, $"%{description}%"));
            }
            if (type is not null)
            {
                query = query.Where(c=>c.Type == type);
            }
            return await query.ToListAsync(cancellationToken);
        }
        public async Task<List<Category>> FindByDescriptionKeywordsAsync(string[] keywords, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c=>keywords.Any(k=>EF.Functions.ILike(c.Description,$"%{k}%")))
                .ToListAsync(cancellationToken);
        }
        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }

    }
}
