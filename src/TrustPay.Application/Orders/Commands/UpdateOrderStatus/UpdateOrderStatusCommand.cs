namespace TrustPay.Application.Orders.Commands.UpdateOrderStatus;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

public record UpdateOrderStatusCommand(
    Guid Id,
    OrderStatus NewStatus
) : IRequest<Result>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Заказ не найден.");
        }

        var result = order.ChangeStatus(request.NewStatus);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}