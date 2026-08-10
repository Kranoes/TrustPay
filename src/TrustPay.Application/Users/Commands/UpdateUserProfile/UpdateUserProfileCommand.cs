namespace TrustPay.Application.Users.Commands.UpdateUserProfile;

using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;

public record UpdateUserProfileCommand(
    Guid UserId,
    string Email,
    string NickName) : IRequest<Result>;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"Пользователь с ID '{request.UserId}' не найден.");
        }

        if (!string.Equals(user.UserEmail, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool isEmailUnique = await _userRepository.IsEmailUnique(request.Email, cancellationToken);
            if (!isEmailUnique)
            {
                return Error.Conflict("User.EmailNotUnique", "Этот email уже занят другим пользователем.");
            }
        }

        if (!string.Equals(user.UserName, request.NickName, StringComparison.OrdinalIgnoreCase))
        {
            bool isNickUnique = await _userRepository.IsNickNameUnique(request.NickName, cancellationToken);
            if (!isNickUnique)
            {
                return Error.Conflict("User.NickNameNotUnique", "Этот никнейм уже занят.");
            }
        }

       
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}