namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.SubCategories.Commands.CreateSubCategory;
using TrustPay.Application.SubCategories.Commands.DeleteSubCategory;
using TrustPay.Application.SubCategories.Commands.UpdateSubCategoryTitle;
using TrustPay.Application.SubCategories.DTOs;
using TrustPay.Application.SubCategories.Queries.GetSubCategoriesByCategoryId;
using TrustPay.Application.SubCategories.Queries.GetSubCategoryById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление подкатегориями товаров и услуг
/// </summary>
[Route("api/sub-categories")]
[Authorize]
public class SubCategoriesController : ApiController
{
    /// <summary>
    /// Получить подкатегорию по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор подкатегории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Подкатегория успешно найдена</response>
    /// <response code="404">Подкатегория с указанным ID не найдена</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SubCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubCategoryByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Получить все подкатегории конкретной категории
    /// </summary>
    /// <param name="categoryId">Идентификатор родительской категории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Список подкатегорий получен</response>
    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SubCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategoryId(
        [FromRoute] Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubCategoriesByCategoryIdQuery(categoryId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Создать новую подкатегорию
    /// </summary>
    /// <param name="command">Данные для создания подкатегории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Подкатегория успешно создана</response>
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
        [FromBody] CreateSubCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Обновить заголовок подкатегории
    /// </summary>
    /// <param name="id">Идентификатор подкатегории</param>
    /// <param name="request">Новый заголовок подкатегории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Заголовок успешно обновлен</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Подкатегория не найдена</response>
    [HttpPut("{id:guid}/title")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTitle(
        [FromRoute] Guid id,
        [FromBody] UpdateSubCategoryTitleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubCategoryTitleCommand(id, request.NewTitle);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Удалить подкатегорию
    /// </summary>
    /// <param name="id">Идентификатор удаляемой подкатегории</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Подкатегория успешно удалена</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
    /// <response code="404">Подкатегория не найдена</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteSubCategoryCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateSubCategoryTitleRequest(string NewTitle);