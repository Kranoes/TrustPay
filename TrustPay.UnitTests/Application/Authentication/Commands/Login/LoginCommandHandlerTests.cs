using System.Reflection.Metadata;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Authentication.Commands.Login;
using TrustPay.Application.Common.Authentication.Commands.RefreshToken;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;


namespace TrustPay.UnitTests.Application.Authentication.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IPasswordHasher _passwordHasherMock;
        private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
        private readonly LoginCommandHandler _handler;
        public LoginCommandHandlerTests()
        {

            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _passwordHasherMock = Substitute.For<IPasswordHasher>();
            _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _handler = new LoginCommandHandler( _userRepositoryMock, _passwordHasherMock, _jwtTokenGeneratorMock, _unitOfWorkMock);

        }
        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var email = "valid@gmail.com";
            var password = "ValidPassword2%";
            var command = new LoginCommand(email,password);

            _userRepositoryMock.GetByEmailAsync(email, Arg.Any<CancellationToken>())
                .Returns((User?)null);
            var result = await _handler.Handle(command,CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Code.Should().Be("Auth.InvalidCredentials");

            _passwordHasherMock.DidNotReceiveWithAnyArgs().VerifyPassword(default!,default!);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            var email = "valid@gmail.com";
            var invalidPassword = "InvalidPassword%2";
            var command = new LoginCommand(email,invalidPassword);
            var user = CreateTestUser(email);

            _userRepositoryMock.GetByEmailAsync(email, Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasherMock.VerifyPassword(invalidPassword, user.PasswordHash)
                .Returns(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Code.Should().Be("Auth.InvalidCredentials");
            _jwtTokenGeneratorMock.DidNotReceiveWithAnyArgs().GenerateRefreshToken();
            _jwtTokenGeneratorMock.DidNotReceiveWithAnyArgs().GenerateAccessToken(default!);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            
        }
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            var email = "valid@gmail.com";
            var password = "ValidPassword2%";
            var expectedAccessToken= "mocked_access_token";
            var expectedRefreshToken = "mocked_refresh_token";
            var command = new LoginCommand(email, password);
            var user = CreateTestUser(email);

            _userRepositoryMock.GetByEmailAsync(email, Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasherMock.VerifyPassword(password, user.PasswordHash)
                .Returns(true);
            _jwtTokenGeneratorMock.GenerateAccessToken(user).Returns(expectedAccessToken);
            _jwtTokenGeneratorMock.GenerateRefreshToken().Returns((expectedRefreshToken, DateTime.UtcNow.AddDays(7)));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(email);
            result.Value.NickName.Should().Be(user.Name);
            result.Value.Token.Should().Be(expectedAccessToken);
            result.Value.RefreshToken.Should().Be(expectedRefreshToken);
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        }
        private static User CreateTestUser()
        {
            return User.Create("test@example.com", "TestUser", "PasswordHash", UserRole.User).Value;
        }
        private static User CreateTestUser(string email)
        {
            return User.Create(email,"TestUser","PasswordHash", UserRole.User).Value;
        }
    }
}
