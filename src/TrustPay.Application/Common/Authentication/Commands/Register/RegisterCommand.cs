namespace TrustPay.Application.Common.Authentication.Commands.Register;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Logging;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

public record RegisterCommand(
    string NickName,
    [property:Sensitive]string Email,
    [property:Sensitive]string Password
) : IRequest<Result<AuthenticationResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserValidationService _userValidationService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IUserValidationService userValidationService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userValidationService = userValidationService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userValidationService.IsEmailTakenAsync(request.Email, cancellationToken))
        {
            return Error.Conflict("User.DuplicateEmail", "Ошибка регистрации: пользователь с таким email уже существует.");
        }

        if (await _userValidationService.IsNickNameTakenAsync(request.NickName, cancellationToken))
        {
            return Error.Conflict("User.DuplicateNickName", "Ошибка регистрации: пользователь с таким никнеймом уже существует.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var userResult = User.Create(request.Email, request.NickName, passwordHash);

        if (userResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(userResult.Error);
        }

        var user = userResult.Value;
        await _userRepository.AddAsync(user, cancellationToken);

        var (refreshToken, expireAt) = _jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, expireAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _userValidationService.RegisterUserEmailAsync(request.Email, cancellationToken);
        await _userValidationService.RegisterUserNickNameAsync(request.NickName, cancellationToken);

        var token = _jwtTokenGenerator.GenerateAccessToken(user);

        var response = new AuthenticationResponse(
            user.Id,
            user.Name,
            user.Email,
            token,
            refreshToken
        );

        return Result<AuthenticationResponse>.Success(response);
    }
}