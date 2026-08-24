namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Categories.Commands.CreateCategory;
using TrustPay.Application.Categories.Commands.DeleteCategory;
using TrustPay.Application.Categories.Commands.UpdateCategory;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Categories.Queries.GetAllCategories;
using TrustPay.Application.Categories.Queries.GetCategoryById;
using TrustPay.Application.Categories.Queries.SearchCategories;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление категориями операций
/// </summary>
[Route("api/categories")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class CategoriesController : ApiController
{
    /// <summary>
    /// Получить категорию по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор категории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Категория успешно найдена</response>
    /// <response code="404">Категория с указанным ID не найдена</response>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить список всех категорий
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Список категорий успешно получен</response>
    [AllowAnonymous]
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Поиск категорий по фильтрам
    /// </summary>
    /// <param name="request">Параметры фильтрации и поиска категорий</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Список найденных категорий</response>
    /// <response code="400">Ошибка в параметрах запроса</response>
    [AllowAnonymous]
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] SearchCategoriesQuery request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Создать новую категорию
    /// </summary>
    /// <param name="request">Данные для создания категории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Категория успешно создана</response>
    /// <response code="400">Ошибка валидации входных данных</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Title, request.Description, request.Type);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Частично обновить данные категории
    /// </summary>
    /// <param name="id">Идентификатор категории</param>
    /// <param name="request">Данные для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Категория успешно обновлена</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Категория не найдена</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Title, request.Description, request.Type);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить категорию по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор удаляемой категории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Категория успешно удалена</response>
    /// <response code="400">Нельзя удалить категорию, содержащую связанные сущности</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Категория не найдена</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateCategoryRequest(string Title, string Description, CategoryType Type);
public record UpdateCategoryRequest(string? Title, string? Description, CategoryType? Type);