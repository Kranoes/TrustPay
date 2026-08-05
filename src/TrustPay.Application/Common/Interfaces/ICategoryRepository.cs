using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Category category, CancellationToken cancellationToken = default);
        Task<List<Category>> FindBySubCategory(Guid subCategoryId, CancellationToken cancellationToken = default);
        Task<Guid?> GetIdByTitle(string title, CancellationToken cancellationToken = default);
        Task<List<Category>> FindByDescriptionKeywordsAsync(string[] keywords, CancellationToken cancellationToken = default);

        void Update(Category category);
        void Delete(Category category);

    }
}
