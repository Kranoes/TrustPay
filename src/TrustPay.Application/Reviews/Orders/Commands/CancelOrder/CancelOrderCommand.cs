namespace TrustPay.Application.Orders.Commands.CancelOrder;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record CancelOrderCommand(Guid OrderId) : IRequest<Result>;
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Заказ не найден.");
        }

        var currentUserId = _currentUserService.UserId;
        var isCustomer = order.CustomerId == currentUserId;
        var isExecutor = order.ExecutorId == currentUserId;
        var isAdmin = _currentUserService.IsAdmin;

        if (!isCustomer && !isExecutor && !isAdmin)
        {
            return Result.Failure("У вас нет прав для отмены этого заказа.");
        }

        var cancelResult = order.Cancel(currentUserId, isAdmin);
        if (cancelResult.IsFailure)
        {
            return cancelResult.Error;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}