namespace TrustPay.Application.Users.Queries.GetPublicUserProfile;

using MediatR;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Users.DTO;
using TrustPay.Domain.Common;

public record GetPublicUserProfileQuery(Guid Id) : IRequest<Result<PublicUserResponse>>;

public class GetPublicUserProfileQueryHandler : IRequestHandler<GetPublicUserProfileQuery, Result<PublicUserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetPublicUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PublicUserResponse>> Handle(GetPublicUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"Пользователь с ID '{request.Id}' не найден.");
        }

        var response = new PublicUserResponse(
            user.Id,
            user.Name,
            user.AvgRating,
            user.CountOfValuations,
            user.CreatedAt);

        return Result.Success(response);
    }
}
