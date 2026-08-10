namespace TrustPay.Application.Users.Queries.GetUserByEmail;

using MediatR;
using TrustPay.Application.Users.DTO;
using TrustPay.Domain.Common;

public record GetUserByEmailQuery(string Email) : IRequest<Result<UserResponse>>;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFoundByEmail", $"Пользователь с Email '{request.Email}' не найден.");
        }

        var response = new UserResponse(
            user.Id,
            user.Wallet?.Id,
            user.UserEmail,
            user.UserName,
            user.Role);

        return Result.Success(response);
    }
}