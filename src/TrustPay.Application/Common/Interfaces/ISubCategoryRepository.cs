using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces;

public interface ISubCategoryRepository
{
    Task<SubCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(SubCategory subCategory, CancellationToken cancellationToken = default);
    void Update(SubCategory subCategory);
    void Delete(SubCategory subCategory);
}