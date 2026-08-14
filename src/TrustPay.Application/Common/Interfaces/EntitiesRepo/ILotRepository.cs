using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces.EntitiesRepo;

public interface ILotRepository
{
    Task<Lot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Lot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Lot>> GetBySubCategoryIdAsync(Guid subCategoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Lot lot, CancellationToken cancellationToken = default);
    void Update(Lot lot);
    void Delete(Lot lot);
}