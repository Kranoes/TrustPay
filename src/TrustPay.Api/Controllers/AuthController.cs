using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Common.Authentication.Commands.RefreshToken;
using TrustPay.Application.Common.Authentication.Commands.Register;
using TrustPay.Application.Common.Authentication.Queries.Login;

namespace TrustPay.Api.Controllers
{
    [Route("api/auth")] 
    public class AuthController : ApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command,CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command,cancellationToken);
            
            return HandleResult(result);
        }
        [HttpPost("login")]
        
        public async Task<IActionResult> Login([FromBody] LoginQuery query, CancellationToken cancellationToken)
        {
            var result  = await Mediator.Send(query,cancellationToken);
            return HandleResult(result);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh ([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }
    }
}
