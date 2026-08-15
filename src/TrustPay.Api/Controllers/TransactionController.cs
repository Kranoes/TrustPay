using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Common.Transactions.Queries.GetWalletTransactions;
using TrustPay.Application.Transactions.Queries.GetTransactionById;

namespace TrustPay.Api.Controllers;

public class TransactionsController : ApiController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTransactionByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("wallet/{walletId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWalletId(
        Guid walletId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWalletTransactionsQuery(walletId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}