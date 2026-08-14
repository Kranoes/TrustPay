using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly TrustPayDbContext _context;

    public OrderRepository(TrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Dispute)
            .Include(o => o.Review)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByExecutorIdAsync(Guid executorId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.ExecutorId == executorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByDisputeIdAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Dispute != null && o.Dispute.Id == disputeId, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        _context.Orders.Remove(order);
    }
}