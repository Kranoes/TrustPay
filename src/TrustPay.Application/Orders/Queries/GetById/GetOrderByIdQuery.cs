namespace TrustPay.Application.Orders.Queries.GetById;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Orders.DTOs;
using TrustPay.Domain.Common;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderResponse>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Заказ не найден.");
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