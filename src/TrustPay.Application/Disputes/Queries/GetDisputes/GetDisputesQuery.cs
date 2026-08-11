namespace TrustPay.Application.Disputes.Queries.GetDisputes;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Disputes.DTOs;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

public record GetDisputesQuery(
    DisputeStatus? Status = null,
    Guid? CustomerId = null,
    Guid? ExecutorId = null,
    Guid? ArbitratorId = null,
    string[]? Keywords = null
) : IRequest<Result<List<DisputeResponse>>>;

public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, Result<List<DisputeResponse>>>
{
    private readonly ITrustPayDbContext _context;

    public GetDisputesQueryHandler(ITrustPayDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DisputeResponse>>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Disputes.AsNoTracking().AsQueryable();
       if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }
        if (request.CustomerId.HasValue)
        {
            query = query.Where(d => d.CustomerId == request.CustomerId.Value);
        }
        if (request.ExecutorId.HasValue)
        {
            query = query.Where(d => d.ExecutorId == request.ExecutorId.Value);
        }
        if (request.ArbitratorId.HasValue)
        {
            query = query.Where(d => d.ArbitratorId == request.ArbitratorId.Value);
        }
        if (request.Keywords is not null && request.Keywords.Length>0)
        {
            query = query.Where(d => request.Keywords.Any(k => EF.Functions.Like(d.Reason, $"%{k}%")));
        }
        

        var response = await query.Select(d => new DisputeResponse(
            d.Id,
            d.CustomerId,
            d.ExecutorId,
            d.ArbitratorId,
            d.Reason,
            d.Status,
            d.CreatedAt
        )).ToListAsync(cancellationToken);

        return Result.Success(response);
    }
}