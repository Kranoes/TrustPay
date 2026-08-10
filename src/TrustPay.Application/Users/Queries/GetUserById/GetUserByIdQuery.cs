namespace TrustPay.Application.Users.Queries.GetUserById;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Users.DTO;
using TrustPay.Application.Users.Queries;
using TrustPay.Domain.Common;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"Пользователь с ID '{request.Id}' не найден.");
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

