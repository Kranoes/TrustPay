namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Reviews.Commands.CreateReview;
using TrustPay.Application.Reviews.Commands.DeleteReview;
using TrustPay.Application.Reviews.Commands.UpdateReview;
using TrustPay.Application.Reviews.DTOs;
using TrustPay.Application.Reviews.Queries.GetById;
using TrustPay.Application.Reviews.Queries.GetByOrderId;

/// <summary>
/// Управление отзывами
/// </summary>
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ApiController
{
    /// <summary>
    /// Получить отзыв по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор отзыва</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Отзыв найден</response>
    /// <response code="404">Отзыв не найден</response>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить отзыв по идентификатору заказа
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Отзыв найден</response>
    /// <response code="404">Отзыв к данному заказу не найден</response>
    [AllowAnonymous]
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId([FromRoute] Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetReviewByOrderIdQuery(orderId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Оставить отзыв к выполненному заказу
    /// </summary>
    /// <param name="request">Текст и оценка отзыва</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Отзыв успешно опубликован</response>
    /// <response code="400">Некорректная оценка или статус заказа не позволяет оставить отзыв</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="409">Отзыв к этому заказу уже существует</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            request.OrderId,
            request.Title,
            request.Message,
            request.Rating);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Обновить ранее оставленный отзыв
    /// </summary>
    /// <param name="id">Идентификатор отзыва</param>
    /// <param name="request">Новый текст и оценка</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Отзыв успешно обновлен</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Отзыв не найден</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(id, request.Title, request.Message, request.Rating);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить отзыв
    /// </summary>
    /// <param name="id">Идентификатор отзыва</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Отзыв успешно удален</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Отзыв не найден</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateReviewRequest(Guid OrderId, string Title, string Message, int Rating);
public record UpdateReviewRequest(string Title, string Message, int Rating);