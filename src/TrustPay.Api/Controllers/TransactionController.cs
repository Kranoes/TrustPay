namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions;
using TrustPay.Application.Transactions.Queries.GetTransactionById;

/// <summary>
/// Управление и просмотр финансовых транзакций
/// </summary>
[Authorize]
[Route("api/transactions")]
public class TransactionsController : ApiController
{
    /// <summary>
    /// Получить детали транзакции по ID
    /// </summary>
    /// <param name="id">Идентификатор транзакции</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Информация о транзакции</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к просмотру данной транзакции</response>
    /// <response code="404">Транзакция не найдена</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTransactionByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить историю транзакций конкретного кошелька
    /// </summary>
    /// <param name="walletId">Идентификатор кошелька</param>
    /// <param name="pageNumber">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Страница с транзакциями кошелька</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к просмотру истории данного кошелька</response>
    /// <response code="404">Кошелек не найден</response>
    [HttpGet("wallet/{walletId:guid}")]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWalletId(
        [FromRoute] Guid walletId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWalletTransactionsQuery(walletId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}