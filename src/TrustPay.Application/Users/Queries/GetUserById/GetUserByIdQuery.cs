namespace TrustPay.Application.Users.Queries.GetUserById;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries;
using TrustPay.Domain.Common;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByIdQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            return Error.Forbidden("User.Forbidden", "Вы можете просматривать только свой профиль.");
        }

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"Пользователь с ID '{request.Id}' не найден.");
        }

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.Name,
            user.Role);

        return Result.Success(response);
    }
}

