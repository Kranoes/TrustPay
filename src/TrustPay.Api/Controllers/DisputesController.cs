namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
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
[Authorize]
public class DisputesController : ApiController
{
    /// <summary>
    /// Получить список споров с фильтрацией
    /// </summary>
    /// <param name="status">Фильтр по статусу спора</param>
    /// <param name="customerId">Фильтр по ID заказчика</param>
    /// <param name="executorId">Фильтр по ID исполнителя</param>
    /// <param name="arbitratorId">Фильтр по ID арбитра</param>
    /// <param name="keywords">Ключевые слова для поиска в описании</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Список споров успешно получен</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DisputeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    /// <param name="id">Идентификатор спора</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Детали спора найдены</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к просмотру данного спора</response>
    /// <response code="404">Спор не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DisputeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDisputeByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Открыть новый спор по заказу
    /// </summary>
    /// <param name="request">Данные для открытия спора</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Спор успешно создан</response>
    /// <response code="400">Ошибка валидации или невозможно выставить спор по заказу</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDisputeCommand(
            request.OrderId,
            request.Reason);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Изменить статус спора
    /// </summary>
    /// <param name="id">Идентификатор спора</param>
    /// <param name="request">Новый статус спора</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Статус спора успешно изменен</response>
    /// <response code="400">Недопустимая смена статуса</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Arbitrator или Admin)</response>
    /// <response code="404">Спор не найден</response>
    [Authorize(Roles = $"{nameof(UserRole.Arbitrator)},{nameof(UserRole.Admin)}")]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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