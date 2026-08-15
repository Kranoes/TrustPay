using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

namespace TrustPay.Application.Common.Interfaces.EntitiesRepo
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Category category, CancellationToken cancellationToken = default);
        Task<Guid?> GetIdByTitle(string title, CancellationToken cancellationToken = default);
        Task<List<Category>> FindByDescriptionKeywordsAsync(string[] keywords, CancellationToken cancellationToken = default);
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Category>> SearchAsync(string? title, string? description, CategoryType? type, CancellationToken cancellationToken = default);
        void Update(Category category);
        void Delete(Category category);

    }
}
