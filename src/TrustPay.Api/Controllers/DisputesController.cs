namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Disputes.Commands.ChangeDisputeStatus;
using TrustPay.Application.Disputes.Commands.CreateDispute;
using TrustPay.Application.Disputes.DTO;
using TrustPay.Application.Disputes.DTOs;
using TrustPay.Application.Disputes.Queries.GetDisputeById;
using TrustPay.Application.Disputes.Queries.GetDisputes;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление спорами и арбитражем
/// </summary>
[Route("api/disputes")]
public class DisputesController : ApiController
{
    /// <summary>
    /// Получить список споров (всех или по фильтрам)
    /// </summary>
    /// <remarks>
    /// Примеры:
    /// - GET /api/disputes
    /// - GET /api/disputes?status=UnderReview
    /// - GET /api/disputes?keywords=quality&amp;keywords=delay
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<DisputeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DisputeStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? executorId,
        [FromQuery] Guid? arbitratorId,
        [FromQuery] string[]? keywords,
        CancellationToken cancellationToken)
    {
        var query = new GetDisputesQuery(status, customerId, executorId, arbitratorId, keywords);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить спор по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DisputeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDisputeByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Открыть новый спор
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
    [FromBody] CreateDisputeRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateDisputeCommand(
            request.OrderId,
            request.CustomerId,
            request.ExecutorId,
            request.Reason);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Изменить статус спора (Арбитраж / Разрешение)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeDisputeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeDisputeStatusCommand(id, request.Status);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}


