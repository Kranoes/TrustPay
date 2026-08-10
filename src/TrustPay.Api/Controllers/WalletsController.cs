namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries;
using TrustPay.Application.Users.Queries.GetUserByWalletId;
using TrustPay.Application.Wallets.Commands.CreateWallet;
using TrustPay.Application.Wallets.Commands.DepositMoney;
using TrustPay.Application.Wallets.Commands.FreezeWallet;
using TrustPay.Application.Wallets.Commands.TransferMoney;
using TrustPay.Application.Wallets.Commands.UnfreezeWallet;
using TrustPay.Application.Wallets.Commands.WithdrawMoney;
using TrustPay.Application.Wallets.Queries.GetWalletById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление кошельками и финансовыми ресурсами
/// </summary>
[Route("api/wallets")]
public class WalletsController : ApiController
{
    /// <summary>
    /// Создать новый кошелек
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetWalletByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить владельца кошелька (Вложенный ресурс)
    /// </summary>
    [HttpGet("{walletId:guid}/user")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOwner(
        [FromRoute] Guid walletId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByWalletIdQuery(walletId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Внести средства (Создать пополнение)
    /// </summary>
    [HttpPost("{id:guid}/deposits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Списать средства (Создать вывод)
    /// </summary>
    [HttpPost("{id:guid}/withdrawals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// Перевести средства на другой кошелек (Создать перевод)
    /// </summary>
    [HttpPost("{id:guid}/transfers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

// Request DTOs
public record CreateWalletRequest(Guid UserId, decimal InitialAmount = 0, string Currency = "RUB");
public record DepositMoneyRequest(decimal Amount, string Currency = "RUB");
public record WithdrawMoneyRequest(decimal Amount, string Currency = "RUB");
public record TransferMoneyRequest(Guid RecipientWalletId, decimal Amount, string Currency = "RUB");
public record ChangeWalletStatusRequest(WalletStatus Status);