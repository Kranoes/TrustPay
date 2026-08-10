namespace TrustPay.Application.Disputes.Queries.GetDisputes;

using MediatR;
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
    private readonly IDisputeRepository _disputeRepository;

    public GetDisputesQueryHandler(IDisputeRepository disputeRepository)
    {
        _disputeRepository = disputeRepository;
    }

    public async Task<Result<List<DisputeResponse>>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var disputes = await _disputeRepository.GetFilteredAsync(
            request.Status,
            request.CustomerId,
            request.ExecutorId,
            request.ArbitratorId,
            request.Keywords,
            cancellationToken);

        var response = disputes.Select(d => new DisputeResponse(
            d.Id,
            d.CustomerId,
            d.ExecutorId,
            d.ArbitratorId,
            d.Reason,
            d.Status,
            d.CreatedAt
        )).ToList();

        return Result<List<DisputeResponse>>.Success(response);
    }
}