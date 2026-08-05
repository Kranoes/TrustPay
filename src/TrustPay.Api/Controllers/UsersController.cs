using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Users.Commands.CreateUser;
using TrustPay.Application.Users.Queires;
using TrustPay.Application.Wallets.Queries.GetWalletByUserId;

namespace TrustPay.Api.Controllers
{
    public class UsersController : ApiController
    {
        private readonly ISender _mediator;

        public UsersController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateUserCommand(request.Email, request.NickName);
            var result = await _mediator.Send(command, cancellationToken);

            return HandleCreatedResult(
                result,
                nameof(GetById),
                new { id = result.IsSuccess ? result.Value : Guid.Empty });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            return HandleResult(result);
        }
        [HttpGet("{id:guid}/wallet")]
        public async Task<IActionResult> GetUserWallet(
    [FromRoute] Guid id,
    CancellationToken cancellationToken)
        {
            var query = new GetWalletByUserIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            return HandleResult(result);
        }
    }

    public record CreateUserRequest(string Email, string NickName);
}