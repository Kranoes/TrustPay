namespace TrustPay.Application.Disputes.Commands.ChangeDisputeStatus;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

public record ChangeDisputeStatusCommand(
    Guid DisputeId,
    DisputeStatus NewStatus
) : IRequest<Result<Unit>>;

public class ChangeDisputeStatusCommandHandler : IRequestHandler<ChangeDisputeStatusCommand, Result<Unit>>
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ChangeDisputeStatusCommandHandler(
        IDisputeRepository disputeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _disputeRepository = disputeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(ChangeDisputeStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;

        var dispute = await _disputeRepository.GetByIdAsync(request.DisputeId, cancellationToken);
        if (dispute == null)
        {
            return Result<Unit>.Failure("Спор не найден.");
        }

        var result = request.NewStatus switch
        {
            DisputeStatus.ResolvedForBuyer => dispute.ResolveInFavorOfCustomer(userId, isAdmin),
            DisputeStatus.ResolvedForSeller => dispute.ResolveInFavorOfExecutor(userId, isAdmin),
            DisputeStatus.Cancelled => dispute.Cancel(userId, isAdmin),
            _ => Result.Failure($"Переход в статус {request.NewStatus} через данный эндпоинт не поддерживается.")
        };

        if (result.IsFailure)
        {
            return Result<Unit>.Failure(result.Error);
        }

        _disputeRepository.Update(dispute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}