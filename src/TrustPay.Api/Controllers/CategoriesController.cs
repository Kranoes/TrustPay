namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Categories.Commands.CreateCategory;
using TrustPay.Application.Categories.Commands.DeleteCategory;
using TrustPay.Application.Categories.Commands.UpdateCategory;
using TrustPay.Application.Categories.DTOs;
using TrustPay.Application.Categories.Queries.GetCategories;
using TrustPay.Application.Categories.Queries.GetCategoryById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление категориями операций
/// </summary>
[Route("api/categories")]
public class CategoriesController : ApiController
{
    /// <summary>
    /// Получить список категорий (всех или с фильтрацией)
    /// </summary>
    /// <remarks>
    /// Примеры:
    /// - GET /api/categories (все)
    /// - GET /api/categories?subCategoryId=guid
    /// - GET /api/categories?keywords=food&amp;keywords=shop
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string[]? keywords,
        [FromQuery] Guid? subCategoryId,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery(keywords, subCategoryId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить категорию по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать новую категорию
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// Обновить существующую категорию
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Title, request.Description, request.Type);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить категорию по идентификатору
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

// Request DTOs
public record CreateCategoryRequest(string Title, string Description, CategoryType Type);
public record UpdateCategoryRequest(string Title, string Description, CategoryType Type);