namespace TrustPay.Application.Orders.Commands.CreateOrder;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;

public record CreateOrderCommand(
    Guid LotId,
    int Quantity
) : IRequest<Result<Guid>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILotRepository _lotRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork,ILotRepository lotRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _lotRepository = lotRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.UserId;
        if (customerId == Guid.Empty)
        {
            return Result<Guid>.Failure("Пользователь не авторизован.");
        }
        var lot = await _lotRepository.GetByIdAsync(request.LotId,cancellationToken);
        if (lot is null)
        {
            return Result<Guid>.Failure("Лот не найден.");
        }
        if (customerId == lot.UserId)
        {
            return Result<Guid>.Failure("Исполнитель и заказчик не могуть быть одним лицом.");
        }
        var orderResult = Order.Create(
            customerId,
            lot.UserId,
            lot.Id,
            request.Quantity,
            lot.Cost);

        if (orderResult.IsFailure)
        {
            return Result<Guid>.Failure(orderResult.Error);
        }

        var order = orderResult.Value;

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}