using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Common.Authentication.Commands.Register
{
    public record RegisterCommand
        (string NickName,
        string Email,
        string Password)
        : IRequest<Result<AuthenticationResponse>>;
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;
        public RegisterCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
        }
        public async Task<Result<AuthenticationResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            
            if (!await _userRepository.IsEmailUniqueAsync(request.Email,cancellationToken))
            {
                return Result.Failure<AuthenticationResponse>("Ошибка регистрации: пользователь с таким email уже существует.");
            }
            if (!await _userRepository.IsNickNameUniqueAsync(request.NickName,cancellationToken))
            {
                return Result.Failure<AuthenticationResponse>("Ошибка регистрации: пользователь с таким никнеймом уже существует.");

            }
            var passwordHash = _passwordHasher.HashPassword(request.Password);
            var userResult = User.Create(request.Email,request.NickName,passwordHash);
            if (userResult.IsFailure)
            {
                return Result.Failure<AuthenticationResponse>(userResult.Error);
            }
            var user = userResult.Value;
            await _userRepository.AddAsync(user, cancellationToken);
            var (refreshToken, expireAt) = _jwtTokenGenerator.GenerateRefreshToken();
            user.AddRefreshToken(refreshToken, expireAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
}
