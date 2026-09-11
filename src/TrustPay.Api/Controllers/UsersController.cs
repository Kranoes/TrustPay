namespace TrustPay.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrustPay.Application.Users.Commands.ChangeUserRole;
using TrustPay.Application.Users.Commands.CreateUser;
using TrustPay.Application.Users.Commands.UpdateUserProfile;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries.GetUserByEmail;
using TrustPay.Application.Users.Queries.GetUserById;
using TrustPay.Domain.Enums;

/// <summary>
/// Управление пользователями
/// </summary>
[Authorize]
[Route("api/users")]
public class UsersController : ApiController
{
    /// <summary>
    /// Создать нового пользователя (Администратор)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.NickName, request.passwordHash, request.Role);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Найти пользователя по Email
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Обновить данные профиля пользователя
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserProfileCommand(id, request.Email, request.NickName);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Изменить роль пользователя (Администратор)
    /// </summary>
    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(
        [FromRoute] Guid id,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserRoleCommand(id, request.NewRole);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}

public record CreateUserRequest(string Email, string NickName, string passwordHash, UserRole Role = UserRole.User);
public record UpdateUserProfileRequest(string Email, string NickName);
public record ChangeUserRoleRequest(UserRole NewRole);