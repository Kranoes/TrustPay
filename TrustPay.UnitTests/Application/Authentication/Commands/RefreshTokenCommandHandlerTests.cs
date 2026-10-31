using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Authentication.Commands.RefreshToken;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

namespace TrustPay.UnitTests.Application.Authentication.Commands
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
        private readonly RefreshTokenCommandHandler _handler;
        public RefreshTokenCommandHandlerTests()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();
            _handler = new RefreshTokenCommandHandler(
                _jwtTokenGeneratorMock,
                _userRepositoryMock,
                _unitOfWorkMock
            );
            
        }
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            var token = "unknow token";
            var command = new RefreshTokenCommand(token);
             _userRepositoryMock.GetByRefreshTokenAsync(token,Arg.Any<CancellationToken>())
                .Returns((User?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);

            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsExpiredOrInvalid()
        {
            var token = "expired token";
            var OldestTokenHash = "expired-token-hash";
            var newToken = "new-token";
            var newTokenHash = "new-token-hash";
            var command = new RefreshTokenCommand(token);
            var testUser = CreateTestUser(Guid.NewGuid());
            testUser.AddRefreshToken(OldestTokenHash,DateTime.UtcNow.AddDays(5));
            _jwtTokenGeneratorMock.GetRefreshTokenHash(token)
                .Returns(OldestTokenHash);
            _userRepositoryMock.GetByRefreshTokenAsync(OldestTokenHash, Arg.Any<CancellationToken>())
                .Returns(testUser);
            _jwtTokenGeneratorMock.GetRefreshTokenHash(newToken)
                .Returns(newTokenHash);
            testUser.AddRefreshToken(newTokenHash, DateTime.UtcNow.AddDays(7));
            testUser.RevokeRefreshToken(OldestTokenHash);

            var result = await _handler.Handle(command,CancellationToken.None);
            var revokedToken = testUser.RefreshTokens.Single(t => t.TokenHash == OldestTokenHash);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Unauthorized);
            revokedToken.IsRevoked.Should().BeTrue();
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());


        }
        [Fact]
        public async Task Handle_ShouldReturnSuccessWithTokens_WhenRefreshTokenIsValid()
        {
            var oldRefreshToken = "valid refresh token";
            var oldTokenHash = "valid token hash old";
            var newAcessToken = "new acess token";
            var newRefreshToken = "new Refresh Token";
            var tokenExpiration = DateTime.UtcNow.AddDays(7);
            var newTokenHash = "hashed token";
            var command = new RefreshTokenCommand(oldRefreshToken);
            var testUser = CreateTestUser(Guid.NewGuid());
            _jwtTokenGeneratorMock.GetRefreshTokenHash(oldRefreshToken)
                .Returns(oldTokenHash);
            testUser.AddRefreshToken(oldTokenHash, DateTime.UtcNow.AddDays(1));
            _userRepositoryMock.GetByRefreshTokenAsync(oldTokenHash, Arg.Any<CancellationToken>())
                .Returns(testUser);
            _jwtTokenGeneratorMock.GenerateAccessToken(testUser)
                .Returns(newAcessToken);
            _jwtTokenGeneratorMock.GenerateRefreshToken()
                .Returns((newRefreshToken, newTokenHash, tokenExpiration));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(new AuthenticationResponse(
                Id: testUser.Id,
                NickName: testUser.Name,
                Email: testUser.Email,
                Token: newAcessToken,
                RefreshToken: newRefreshToken
                ));
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        
        private static User CreateTestUser(Guid userId)
        {
            return User.Create("test@example.com", "TestUser", "PasswordHash", UserRole.User).Value;
        }
    }
}
