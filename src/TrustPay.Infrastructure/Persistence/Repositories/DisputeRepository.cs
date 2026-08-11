namespace TrustPay.Infrastructure.Persistence.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public class DisputeRepository : IDisputeRepository
{
    private readonly TrustPayDbContext _context;

    public DisputeRepository(TrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Dispute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
   
    public async Task<List<Dispute>> FindByReasonKeywordsAsync(string[] keywords, CancellationToken cancellationToken = default)
    {
        var query = _context.Disputes.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (customerId.HasValue)
            query = query.Where(d => d.CustomerId == customerId.Value);

        if (executorId.HasValue)
            query = query.Where(d => d.ExecutorId == executorId.Value);

        if (arbitratorId.HasValue)
            query = query.Where(d => d.ArbitratorId == arbitratorId.Value);

        if (keywords != null && keywords.Length > 0)
        {
            query = query.Where(d => keywords.Any(k => EF.Functions.ILike(d.Reason, $"%{k}%")));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetCustomerIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .AsNoTracking()
            .Where(d => d.Id == disputeId)
            .Select(d => (Guid?)d.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetExecutorIdByDisputeAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .AsNoTracking()
            .Where(d => d.Id == disputeId)
            .Select(d => (Guid?)d.ExecutorId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DisputeStatus?> GetStatusOfDisputeByIdAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .AsNoTracking()
            .Where(d => d.Id == disputeId)
            .Select(d => (DisputeStatus?)d.Status)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Выбирает админа/арбитра с наименьшим количеством активных споров
    /// </summary>
    public async Task<Guid?> GetAvailableArbitratorIdAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Arbitrator)
            .Select(u => new
            {
                u.Id,
                ActiveDisputesCount = _context.Disputes.Count(d => d.ArbitratorId == u.Id && d.Status == DisputeStatus.UnderReview)
            })
            .OrderBy(u => u.ActiveDisputesCount)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Dispute dispute, CancellationToken cancellationToken = default)
    {
        await _context.Disputes.AddAsync(dispute, cancellationToken);
    }

    public void Update(Dispute dispute)
    {
        _context.Disputes.Update(dispute);
    }
}