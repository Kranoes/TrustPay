namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Lots.Commands.CreateLot;
using TrustPay.Application.Lots.Commands.DeleteLot;
using TrustPay.Application.Lots.Commands.UpdateLot;
using TrustPay.Application.Lots.DTOs;
using TrustPay.Application.Lots.Queries.GetById;
using TrustPay.Application.Lots.Queries.GetLotsBySubCategoryId;
using TrustPay.Application.Lots.Queries.GetLotsByUserId;

/// <summary>
/// Управление лотами
/// </summary>
[Route("api/lots")]
public class LotsController : ApiController
{
    /// <summary>
    /// Получить лот по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить список лотов пользователя
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(List<LotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetLotsByUserIdQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать новый лот
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLotRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLotCommand(
            request.UserId,
            request.SubCategoryId,
            request.Title,
            request.Amount,
            request.Currency,
            request.ItemsCount);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Обновить данные лота
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateLotRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLotCommand(
            id,
            request.Title,
            request.Amount,
            request.Currency,
            request.ItemsCount);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить лот
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteLotCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
    /// <summary>
    /// Получить список лотов по подкатегории
    /// </summary>
    [HttpGet("subcategory/{subCategoryId:guid}")]
    [ProducesResponseType(typeof(List<LotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubCategoryId([FromRoute] Guid subCategoryId, CancellationToken cancellationToken)
    {
        var query = new GetLotsBySubCategoryIdQuery(subCategoryId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateLotRequest(
    Guid UserId,
    Guid SubCategoryId,
    string Title,
    decimal Amount,
    string Currency,
    int ItemsCount);

public record UpdateLotRequest(
    string Title,
    decimal Amount,
    string Currency,
    int ItemsCount);