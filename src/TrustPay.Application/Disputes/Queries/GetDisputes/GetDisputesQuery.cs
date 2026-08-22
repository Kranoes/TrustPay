namespace TrustPay.Application.Disputes.Queries.GetDisputes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
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
    private readonly ICurrentUserService _currentUserService;

    public GetDisputesQueryHandler(ITrustPayDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<DisputeResponse>>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var isPrivileged = _currentUserService.IsAdmin || _currentUserService.IsArbitrator;

        var query = _context.Disputes.AsNoTracking().AsQueryable();

        if (!isPrivileged)
        {
            query = query.Where(d => d.CustomerId == currentUserId || d.ExecutorId == currentUserId);
        }
        else
        {
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
        }

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        if (request.Keywords is not null && request.Keywords.Length > 0)
        {
            foreach (var keyword in request.Keywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var pattern = $"%{keyword.Trim()}%";
                    query = query.Where(d => EF.Functions.Like(d.Reason, pattern));
                }
            }
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