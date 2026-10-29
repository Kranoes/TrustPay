using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Logging;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Common.Authentication.Commands.RefreshToken
{
    public record RefreshTokenCommand([property:Sensitive]string RefreshToken) : IRequest<Result<AuthenticationResponse>>;
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<AuthenticationResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var latestTokenHash = _jwtTokenGenerator.GetRefreshTokenHash(request.RefreshToken);
            var user = await _userRepository.GetByRefreshTokenAsync(latestTokenHash, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("User.NotFound","Пользователь с данным токеном не найден.");
            }
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.TokenHash == latestTokenHash && rt.IsActive);
            if (activeRefreshToken is null)
            {
                return Error.Unauthorized("Auth.ExpiredToken","Срок действия Refresh Token истек или токен неактивен.");
            }
            var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var (newRefreshToken,RefreshtokenHash,expireAt) = _jwtTokenGenerator.GenerateRefreshToken();
           var resultRevoke = user.RevokeRefreshToken(latestTokenHash);
            if (resultRevoke.IsFailure)
            {
                return resultRevoke.Error;
            }
            var resultAdd =user.AddRefreshToken(RefreshtokenHash, expireAt);
            if (resultAdd.IsFailure)
            {
                return resultAdd.Error;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var response = new AuthenticationResponse(
                user.Id,
                user.Name,
                user.Email,
                newAccessToken,
                newRefreshToken
                );
            return Result<AuthenticationResponse>.Success(response);
        }
    }
}
