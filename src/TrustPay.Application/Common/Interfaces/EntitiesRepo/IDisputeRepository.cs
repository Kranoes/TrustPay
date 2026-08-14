namespace TrustPay.Application.Common.Interfaces.EntitiesRepo;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public interface IDisputeRepository
{
    Task<Dispute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<Dispute>> FindByReasonKeywordsAsync(string[] keywords, CancellationToken cancellationToken = default);
    Task<Guid?> GetCustomerIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<Guid?> GetExecutorIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<DisputeStatus?> GetStatusOfDisputeByIdAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<Guid?> GetAvailableArbitratorIdAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Dispute dispute, CancellationToken cancellationToken = default);
    void Update(Dispute dispute);
}