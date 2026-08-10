namespace TrustPay.Application.Disputes.Queries.GetDisputeById;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Disputes.DTOs;
using TrustPay.Domain.Common;

public record GetDisputeByIdQuery(Guid Id) : IRequest<Result<DisputeResponse>>;

public class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, Result<DisputeResponse>>
{
    private readonly IDisputeRepository _disputeRepository;

    public GetDisputeByIdQueryHandler(IDisputeRepository disputeRepository)
    {
        _disputeRepository = disputeRepository;
    }

    public async Task<Result<DisputeResponse>> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var dispute = await _disputeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (dispute == null)
        {
            return Result<DisputeResponse>.Failure("Спор не найден.");
        }

        var response = new DisputeResponse(
            dispute.Id,
            dispute.CustomerId,
            dispute.ExecutorId,
            dispute.ArbitratorId,
            dispute.Reason,
            dispute.Status,
            dispute.CreatedAt
        );

        return Result<DisputeResponse>.Success(response);
    }
}