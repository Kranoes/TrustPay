namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Common.Authentication.Commands.Logout;
using TrustPay.Application.Common.Authentication.Commands.RefreshToken;
using TrustPay.Application.Common.Authentication.Commands.Register;
using TrustPay.Application.Common.Authentication.Queries.Login;

/// <summary>
/// Аутентификация и регистрация пользователей
/// </summary>
[Route("api/auth")]
public class AuthController : ApiController
{
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="command">Данные учетной записи для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Некорректные данные или ошибка валидации</response>
    /// <response code="409">Пользователь с таким email уже существует</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Вход в систему (Аутентификация)
    /// </summary>
    /// <param name="query">Учетные данные пользователя (логин и пароль)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Успешная аутентификация, возвращены токены доступа</response>
    /// <response code="400">Ошибка валидации запроса</response>
    /// <response code="401">Неверный логин или пароль</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Обновление токена доступа (Refresh Token)
    /// </summary>
    /// <param name="command">Токен обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <response code="200">Токены успешно обновлены</response>
    /// <response code="400">Некорректный запрос</response>
    /// <response code="401">Токен недействителен или истек срок его действия</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
     /// Выход из системы (отзыв Refresh-токена)
     /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
public record LogoutRequest(string RefreshToken);