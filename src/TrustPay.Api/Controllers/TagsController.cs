namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Tags.Commands.CreateTag;
using TrustPay.Application.Tags.Commands.DeleteTag;
using TrustPay.Application.Tags.Commands.UpdateTag;
using TrustPay.Application.Tags.DTOs;
using TrustPay.Application.Tags.Queries.GetAllTags;
using TrustPay.Application.Tags.Queries.GetTagById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление тегами
/// </summary>
[Route("api/tags")]
public class TagsController : ApiController
{
    /// <summary>
    /// Получить список всех тегов с поиском
    /// </summary>
    /// <param name="searchTerm">Строка для поиска по названию тега</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Список тегов успешно получен</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllTagsQuery(searchTerm), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Получить тег по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Тег успешно найден</response>
    /// <response code="404">Тег с указанным ID не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTagByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Создать новый тег
    /// </summary>
    /// <param name="command">Данные для создания тега</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Тег успешно создан</response>
    /// <response code="400">Ошибка валидации входных данных</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTagCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Обновить название тега
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    /// <param name="request">Новое название тега</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Название тега успешно обновлено</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Тег не найден</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTagRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTagCommand(id, request.Name);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить тег
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Тег успешно удален</response>
    /// <response code="400">Ошибка при удалении связанного тега</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Тег не найден</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteTagCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateTagRequest(string Name);