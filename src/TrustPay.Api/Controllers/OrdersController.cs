namespace TrustPay.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Orders.Commands.CreateOrder;
using TrustPay.Application.Orders.Commands.DeleteOrder;
using TrustPay.Application.Orders.Commands.UpdateOrderStatus;
using TrustPay.Application.Orders.DTOs;
using TrustPay.Application.Orders.Queries.GetById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление заказами
/// </summary>
[Route("api/orders")]
public class OrdersController : ApiController
{
    /// <summary>
    /// Получить заказ по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать новый заказ
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.ExecutorId,
            request.LotId,
            request.Quantity,
            request.Amount,
            request.Currency);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Обновить статус заказа
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(id, request.NewStatus);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить заказ
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateOrderRequest(
    Guid CustomerId,
    Guid ExecutorId,
    Guid LotId,
    int Quantity,
    decimal Amount,
    string Currency);

public record UpdateOrderStatusRequest(OrderStatus NewStatus);