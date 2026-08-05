using System;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Review?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    void Update(Review review);
    void Delete(Review review);
}