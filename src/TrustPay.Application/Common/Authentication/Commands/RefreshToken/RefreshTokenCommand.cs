using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;

namespace TrustPay.Application.Common.Authentication.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthenticationResponse>>;
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

            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken,cancellationToken);
            if (user is null)
            {
                return Result.Failure<AuthenticationResponse>("Пользователь с данным токеном не найден.");
            }
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken && !rt.IsExpired);
            if (activeRefreshToken is null)
            {
                return Result.Failure<AuthenticationResponse>("Срок действия Refresh Token истек.");
            }
            var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var (newRefreshToken,expireAt) = _jwtTokenGenerator.GenerateRefreshToken();
            user.RevokeRefreshToken(request.RefreshToken);
            user.AddRefreshToken(newRefreshToken, expireAt);
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
