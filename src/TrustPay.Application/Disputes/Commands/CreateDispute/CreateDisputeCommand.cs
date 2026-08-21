namespace TrustPay.Application.Disputes.Commands.CreateDispute;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record CreateDisputeCommand(
    Guid OrderId,
    string Reason
) : IRequest<Result<Guid>>;

public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, Result<Guid>>
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDisputeCommandHandler(
        IDisputeRepository disputeRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _disputeRepository = disputeRepository;
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty)
        {
            return Result<Guid>.Failure("Пользователь не авторизован.");
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<Guid>.Failure("Заказ не найден.");
        }

        if (order.CustomerId != currentUserId && order.ExecutorId != currentUserId)
        {
            return Result<Guid>.Failure("Вы не можете открыть спор по чужому заказу.");
        }

        var hasActiveDispute = await _disputeRepository.HasActiveDisputeForOrderAsync(request.OrderId, cancellationToken);
        if (hasActiveDispute)
        {
            return Result<Guid>.Failure("По этому заказу уже открыт спор.");
        }

        var disputeResult = Dispute.Create(
            request.OrderId,
            order.CustomerId,
            order.ExecutorId,
            request.Reason);

        if (disputeResult.IsFailure)
        {
            return Result<Guid>.Failure(disputeResult.Error);
        }

        var dispute = disputeResult.Value;

        var arbitratorId = await _disputeRepository.GetAvailableArbitratorIdAsync(cancellationToken);
        if (arbitratorId.HasValue)
        {
            dispute.AssignArbitrator(arbitratorId.Value);
        }

        await _disputeRepository.AddAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(dispute.Id);
    }
}