namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Orders.Commands.CancelOrder;
using TrustPay.Application.Orders.Commands.CompleteOrder;
using TrustPay.Application.Orders.Commands.CreateOrder;
using TrustPay.Application.Orders.Commands.DeleteOrder;
using TrustPay.Application.Orders.Commands.StartOrder;
using TrustPay.Application.Orders.DTOs;
using TrustPay.Application.Orders.Queries.GetById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление заказами
/// </summary>
[Route("api/orders")]
[Authorize]
public class OrdersController : ApiController
{
    /// <summary>
    /// Получить заказ по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.LotId,
            request.Quantity);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Приступить к выполнению заказа (Только Исполнитель)
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new StartOrderCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Подтвердить завершение заказа (Только Заказчик)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteOrderCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Отменить заказ
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Физическое удаление заказа (Только Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateOrderRequest(
    Guid LotId,
    int Quantity);

