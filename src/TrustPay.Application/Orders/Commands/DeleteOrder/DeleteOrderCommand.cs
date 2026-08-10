namespace TrustPay.Application.Orders.Commands.DeleteOrder;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;

public record DeleteOrderCommand(Guid Id) : IRequest<Result>;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Заказ не найден.");
        }

        _orderRepository.Delete(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}