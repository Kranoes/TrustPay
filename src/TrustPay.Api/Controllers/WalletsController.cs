namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries.GetUserByWalletId;
using TrustPay.Application.Wallets.Commands.CreateWallet;
using TrustPay.Application.Wallets.Commands.DepositMoney;
using TrustPay.Application.Wallets.Commands.FreezeWallet;
using TrustPay.Application.Wallets.Commands.TransferMoney;
using TrustPay.Application.Wallets.Commands.UnfreezeWallet;
using TrustPay.Application.Wallets.Commands.WithdrawMoney;
using TrustPay.Application.Wallets.DTOs;
using TrustPay.Application.Wallets.Queries.GetWalletById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление кошельками и финансовыми ресурсами
/// </summary>
[Authorize]
[Route("api/wallets")]
public class WalletsController : ApiController
{
    /// <summary>
    /// Создать новый кошелек
    /// </summary>
    /// <param name="request">Данные для создания кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="201">Кошелек успешно создан</response>
    /// <response code="400">Ошибка валидации входных данных</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав для создания кошелька другому пользователю</response>
    /// <response code="409">У пользователя уже существует кошелек в данной валюте</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWalletRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWalletCommand(request.UserId, request.InitialAmount, request.Currency);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Получить кошелек по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Информация о кошельке</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к данному кошельку</response>
    /// <response code="404">Кошелек не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetWalletByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить владельца кошелька
    /// </summary>
    /// <param name="walletId">Идентификатор кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Данные владельца кошелька</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к информации о данном кошельке</response>
    /// <response code="404">Кошелек не найден</response>
    [HttpGet("{walletId:guid}/user")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOwner(
        [FromRoute] Guid walletId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByWalletIdQuery(walletId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Внести средства на кошелек
    /// </summary>
    /// <param name="id">Идентификатор кошелька</param>
    /// <param name="request">Сумма и валюта пополнения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Средства успешно зачислены</response>
    /// <response code="400">Некорректная сумма или несовпадение валют</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет доступа к управлению балансом этого кошелька</response>
    /// <response code="404">Кошелек не найден</response>
    [HttpPost("{id:guid}/deposits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDeposit(
        [FromRoute] Guid id,
        [FromBody] DepositMoneyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DepositMoneyCommand(id, request.Amount, request.Currency);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Списать средства с кошелька
    /// </summary>
    /// <param name="id">Идентификатор кошелька</param>
    /// <param name="request">Сумма и валюта вывода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Средства успешно списаны</response>
    /// <response code="400">Недостаточно средств или неверная валюта</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Вы не являетесь владельцем кошелька</response>
    /// <response code="404">Кошелек не найден</response>
    [HttpPost("{id:guid}/withdrawals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateWithdrawal(
        [FromRoute] Guid id,
        [FromBody] WithdrawMoneyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new WithdrawMoneyCommand(id, request.Amount, request.Currency);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Перевести средства на другой кошелек
    /// </summary>
    /// <param name="id">Идентификатор кошелька-отправителя</param>
    /// <param name="request">Данные для перевода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Перевод успешно выполнен</response>
    /// <response code="400">Ошибка операции (недостаточно средств, кошелек заблокирован)</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Вы не являетесь владельцем кошелька-отправителя</response>
    /// <response code="404">Один из кошельков не найден</response>
    [HttpPost("{id:guid}/transfers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTransfer(
        [FromRoute] Guid id,
        [FromBody] TransferMoneyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TransferMoneyCommand(id, request.RecipientWalletId, request.Amount, request.Currency);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Изменить статус кошелька (Заморозка / Разморозка)
    /// </summary>
    /// <param name="id">Идентификатор кошелька</param>
    /// <param name="request">Новый статус кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Статус успешно изменен</response>
    /// <response code="400">Недопустимый статус</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав (требуется Admin или Arbitrator)</response>
    /// <response code="404">Кошелек не найден</response>
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Arbitrator)}")]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeWalletStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Status == WalletStatus.Frozen)
        {
            var command = new FreezeWalletCommand(id);
            var result = await Mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        if (request.Status == WalletStatus.Active)
        {
            var command = new UnfreezeWalletCommand(id);
            var result = await Mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        return BadRequest("Недопустимый статус для обновления.");
    }
}

public record CreateWalletRequest(Guid UserId, decimal InitialAmount = 0, string Currency = "RUB");
public record DepositMoneyRequest(decimal Amount, string Currency = "RUB");
public record WithdrawMoneyRequest(decimal Amount, string Currency = "RUB");
public record TransferMoneyRequest(Guid RecipientWalletId, decimal Amount, string Currency = "RUB");
public record ChangeWalletStatusRequest(WalletStatus Status);