using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;

namespace TrustPay.Domain.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Tag?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken cancellationToken = default);
        Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);
    }
}