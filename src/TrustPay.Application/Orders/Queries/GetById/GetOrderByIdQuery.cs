namespace TrustPay.Application.Orders.Queries.GetById;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Orders.DTOs;
using TrustPay.Domain.Common;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderResponse>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Заказ не найден.");
        }

        var currentUserId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;
        var isArbitrator = _currentUserService.IsArbitrator;

        var isParticipant = order.CustomerId == currentUserId || order.ExecutorId == currentUserId;
        var hasAccess = isParticipant || isAdmin || isArbitrator;

        if (!hasAccess)
        {
            return Error.Forbidden("Order.Forbidden", "У вас нет прав для просмотра этого заказа.");
        }

        var response = new OrderResponse(
            order.Id,
            order.CustomerId,
            order.ExecutorId,
            order.LotId,
            order.Quantity,
            order.Price.Amount,
            order.Price.Currency,
            order.Status.ToString(),
            order.CreatedAt
        );

        return Result<OrderResponse>.Success(response);
    }
}