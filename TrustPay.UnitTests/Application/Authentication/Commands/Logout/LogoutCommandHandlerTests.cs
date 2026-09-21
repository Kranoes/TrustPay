using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Authentication.Commands.Logout;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

namespace TrustPay.UnitTests.Application.Authentication.Commands.Logout
{
    public class LogoutCommandHandlerTests
    {
        private readonly IUserRepository _userRepositoryMock;
        private readonly ICurrentUserService _currentUserServiceMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly LogoutCommandHandler _handler;
        public LogoutCommandHandlerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _currentUserServiceMock = Substitute.For<ICurrentUserService>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _handler = new LogoutCommandHandler(
                _userRepositoryMock,
                _currentUserServiceMock,
                _unitOfWorkMock);

        }
        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            var command = new LogoutCommand("valid-refresh-token");
            _currentUserServiceMock.UserId.Returns(Guid.Empty);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Code.Should().Be("Auth.Unauthorized");

            await _userRepositoryMock.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExistInRepository()
        {
            var userId = Guid.NewGuid();
            var command = new LogoutCommand("valid-refresh-token");

            _currentUserServiceMock.UserId.Returns(userId);
            _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>())
                .Returns((User?)null);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("User.NotFound");

            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            
        }
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRevokeRefreshTokenFails()
        {
            var userId = Guid.NewGuid();
            var invalidRefreshToken = "invalid-token";
            var command = new LogoutCommand(invalidRefreshToken);

            var user = CreateTestUser(userId);

            _currentUserServiceMock.UserId.Returns(userId);
            _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>())
                .Returns(user);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task Handle_ShouldReturnSuccessAndSaveChanges_WhenDataIsValid()
        {
            var userId = Guid.NewGuid();
            var refreshToken = "valid-refresh-token";
            var command = new LogoutCommand(refreshToken);

            var user = CreateTestUserWithActiveToken(userId, refreshToken);

            _currentUserServiceMock.UserId.Returns(userId);
            _userRepositoryMock.GetByIdAsync(userId,Arg.Any<CancellationToken>())
                .Returns(user);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        private static User CreateTestUser(Guid userId)
        {
            return User.Create("test@example.com", "TestUser", "PasswordHash",UserRole.User).Value;
        }
        private static User CreateTestUserWithActiveToken(Guid userId, string refreshToken)
        {
            var user = CreateTestUser(userId);
            user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            return user; 
        }
    }
}
