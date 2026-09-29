namespace TrustPay.Application.Users.Commands.CreateUser;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

public record CreateUserCommand(
    string Email,
    string NickName,
    string PasswordHash,
    UserRole Role = UserRole.User) : IRequest<Result<Guid>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserValidationService _userValidationService;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserValidationService userValidationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userValidationService = userValidationService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userValidationService.IsEmailTakenAsync(request.Email, cancellationToken))
        {
            return Error.Conflict("User.EmailNotUnique", "Пользователь с таким email уже существует.");
        }

        if (await _userValidationService.IsNickNameTakenAsync(request.NickName, cancellationToken))
        {
            return Error.Conflict("User.NickNameNotUnique", "Пользователь с таким никнеймом уже существует.");
        }

        var userResult = User.Create(request.Email, request.NickName, request.PasswordHash, request.Role);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        var user = userResult.Value;
        await _userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _userValidationService.RegisterUserEmailAsync(request.Email, cancellationToken);
        await _userValidationService.RegisterUserNickNameAsync(request.NickName, cancellationToken);

        return user.Id;
    }
}