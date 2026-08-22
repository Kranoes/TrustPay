namespace TrustPay.Application.Disputes.Queries.GetDisputeById;

using MediatR;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Disputes.DTOs;
using TrustPay.Domain.Common;

public record GetDisputeByIdQuery(Guid Id) : IRequest<Result<DisputeResponse>>;

public class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, Result<DisputeResponse>>
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDisputeByIdQueryHandler(IDisputeRepository disputeRepository, ICurrentUserService currentUserService)
    {
        _disputeRepository = disputeRepository;
        _currentUserService = currentUserService;
    }
    public async Task<Result<DisputeResponse>> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var isPrivileged = _currentUserService.IsAdmin || _currentUserService.IsArbitrator;

        var dispute = await _disputeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (dispute == null)
        {
            return Result<DisputeResponse>.Failure("Спор не найден.");
        }
        if (!isPrivileged && dispute.CustomerId != currentUserId && dispute.ExecutorId != currentUserId)
        {
            return Result<DisputeResponse>.Failure("У вас нет доступа к просмотру данного спора.");
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