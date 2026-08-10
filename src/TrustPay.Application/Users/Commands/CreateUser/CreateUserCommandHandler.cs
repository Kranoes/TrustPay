namespace TrustPay.Application.Users.Commands.CreateUser;

using FluentValidation;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public record CreateUserCommand(
    string Email,
    string NickName,
    UserRole Role = UserRole.User) : IRequest<Result<Guid>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _userRepository.IsEmailUnique(request.Email, cancellationToken))
        {
            return Error.Conflict("User.EmailNotUnique", "Пользователь с таким email уже существует.");
        }

        if (!await _userRepository.IsNickNameUnique(request.NickName, cancellationToken))
        {
            return Error.Conflict("User.NickNameNotUnique", "Пользователь с таким никнеймом уже существует.");
        }

        var userResult = User.Create(request.Email, request.NickName, request.Role);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        await _userRepository.AddAsync(userResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(userResult.Value.Id);
    }
}