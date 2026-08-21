namespace TrustPay.Api.Controllers;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.SubCategories.Commands.CreateSubCategory;
using TrustPay.Application.SubCategories.Commands.DeleteSubCategory;
using TrustPay.Application.SubCategories.Commands.UpdateSubCategoryTitle;
using TrustPay.Application.SubCategories.DTOs;
using TrustPay.Application.SubCategories.Queries.GetSubCategoriesByCategoryId;
using TrustPay.Application.SubCategories.Queries.GetSubCategoryById;

/// <summary>
/// Управление подкатегориями товаров и услуг.
/// </summary>
public class SubCategoriesController : ApiController
{
    /// <summary>
    /// Создать новую подкатегорию.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Получить подкатегорию по идентификатору.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubCategoryByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Получить все подкатегории конкретной категории.
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(List<SubCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategoryId(
        [FromRoute] Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubCategoriesByCategoryIdQuery(categoryId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Обновить заголовок подкатегории.
    /// </summary>
    [HttpPut("{id:guid}/title")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Удалить подкатегорию.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteSubCategoryCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateSubCategoryTitleRequest(string NewTitle);