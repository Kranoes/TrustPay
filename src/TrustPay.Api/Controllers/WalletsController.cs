using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions;
using TrustPay.Application.Wallets.Commands.CreateWallet;
using TrustPay.Application.Wallets.Commands.DepositMoney;
using TrustPay.Application.Wallets.Commands.TransferMoney;
using TrustPay.Application.Wallets.Queries.GetWalletById;

namespace TrustPay.Api.Controllers
{
    [ApiController]
    [Route("api/v1/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly ISender _mediator;

        public WalletsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWalletRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateWalletCommand(
                request.UserId,
                request.InitialAmount,
                request.Currency);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.Value },
                value: new { id = result.Value });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetWalletByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpPost("{id:guid}/deposits")]
        public async Task<IActionResult> Deposit(
            [FromRoute] Guid id,
            [FromBody] DepositMoneyRequest request,
            CancellationToken cancellationToken)
        {
            var command = new DepositMoneyCommand(
                id,
                request.Amount,
                request.Currency);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok();
        }

        [HttpPost("{id:guid}/transfers")]
        public async Task<IActionResult> Transfer(
            [FromRoute] Guid id,
            [FromBody] TransferMoneyRequest request,
            CancellationToken cancellationToken)
        {
            var command = new TransferMoneyCommand(
                id,
                request.ReceiverWalletId,
                request.Amount,
                request.Currency);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok();
        }

        [HttpGet("{id:guid}/transactions")]
        public async Task<IActionResult> GetWalletTransactions(
            [FromRoute] Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetWalletTransactionsQuery(id, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
    }

    public record CreateWalletRequest(Guid UserId, decimal InitialAmount = 0, string Currency = "RUB");
    public record DepositMoneyRequest(decimal Amount, string Currency = "RUB");
    public record TransferMoneyRequest(Guid ReceiverWalletId, decimal Amount, string Currency = "RUB");
}