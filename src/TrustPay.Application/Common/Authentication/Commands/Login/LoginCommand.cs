using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Logging;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Common.Authentication.Commands.Login
{
    
    public record LoginCommand([property:Sensitive]string Email,[property:Sensitive] string Password) : IRequest<Result<AuthenticationResponse>>;
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email,cancellationToken);
            if (user is null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                return Error.Unauthorized("Auth.InvalidCredentials", "Неверный email или пароль.");
            }
            var token = _jwtTokenGenerator.GenerateAccessToken(user);
            var (refreshToken, expireAt) = _jwtTokenGenerator.GenerateRefreshToken();
            user.AddRefreshToken(refreshToken, expireAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var response = new AuthenticationResponse(
                Id: user.Id,
                NickName: user.Name,
                Email: user.Email,
                Token: token,
                RefreshToken: refreshToken
                );
           
            return Result<AuthenticationResponse>.Success(response);
        }
    }



}
