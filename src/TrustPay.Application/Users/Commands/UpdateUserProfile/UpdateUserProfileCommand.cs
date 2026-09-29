namespace TrustPay.Application.Users.Commands.UpdateUserProfile;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

public record UpdateUserProfileCommand(
    Guid UserId,
    string Email,
    string NickName
) : IRequest<Result>;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserValidationService _userValidationService;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserValidationService userValidationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userValidationService = userValidationService;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"Пользователь с ID '{request.UserId}' не найден.");
        }

        bool isEmailChanged = !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase);
        bool isNickNameChanged = !string.Equals(user.Name, request.NickName, StringComparison.OrdinalIgnoreCase);

        if (!isEmailChanged && !isNickNameChanged)
        {
            return Result.Success();
        }

        if (isEmailChanged)
        {
            if (await _userValidationService.IsEmailTakenAsync(request.Email, cancellationToken))
            {
                return Error.Conflict("User.EmailNotUnique", "Этот email уже занят другим пользователем.");
            }
        }

        if (isNickNameChanged)
        {
            if (await _userValidationService.IsNickNameTakenAsync(request.NickName, cancellationToken))
            {
                return Error.Conflict("User.NickNameNotUnique", "Этот никнейм уже занят.");
            }
        }

        if (isEmailChanged)
        {
            var changeEmailResult = user.ChangeEmail(request.Email);
            if (changeEmailResult.IsFailure)
            {
                return changeEmailResult;
            }
        }

        if (isNickNameChanged)
        {
            var updateProfileResult = user.UpdateProfile(request.NickName);
            if (updateProfileResult.IsFailure)
            {
                return updateProfileResult;
            }
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (isEmailChanged)
        {
            await _userValidationService.RegisterUserEmailAsync(request.Email, cancellationToken);
        }

        if (isNickNameChanged)
        {
            await _userValidationService.RegisterUserNickNameAsync(request.NickName, cancellationToken);
        }

        return Result.Success();
    }
}