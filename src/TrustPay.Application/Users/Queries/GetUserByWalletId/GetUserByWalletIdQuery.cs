namespace TrustPay.Application.Users.Queries.GetUserByWalletId;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries;
using TrustPay.Domain.Common;

public record GetUserByWalletIdQuery(Guid WalletId) : IRequest<Result<UserResponse>>;

public class GetUserByWalletIdQueryHandler : IRequestHandler<GetUserByWalletIdQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByWalletIdQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByWalletIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByWalletIdAsync(request.WalletId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFoundByWallet", $"Пользователь для кошелька с ID '{request.WalletId}' не найден.");
        }

        if (user.Id != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            return Error.Forbidden("User.Forbidden", "Нет прав на просмотр владельца этого кошелька.");
        }

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.Name,
            user.Role);

        return Result.Success(response);
    }
}