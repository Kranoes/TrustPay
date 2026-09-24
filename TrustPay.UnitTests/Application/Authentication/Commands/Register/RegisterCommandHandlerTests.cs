using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Authentication.Commands.Logout;
using TrustPay.Application.Common.Authentication.Commands.RefreshToken;
using TrustPay.Application.Common.Authentication.Commands.Register;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;

namespace TrustPay.UnitTests.Application.Authentication.Commands.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IUserRepository _userRepositoryMock;
        private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
        private readonly IPasswordHasher _passwordHasherMock;
        private readonly RegisterCommandHandler _handler;
        public RegisterCommandHandlerTests()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();
            _passwordHasherMock = Substitute.For<IPasswordHasher>();
            _handler = new RegisterCommandHandler(
                _unitOfWorkMock,
                _userRepositoryMock,
                _jwtTokenGeneratorMock,
                _passwordHasherMock
                );
        }
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenEmailIsNotUnique()
        {
            var email = "duplicate email";
            var nickName = "correct nick name";
            var password = "correct password";
            var command = new RegisterCommand(nickName,email,password);
            _userRepositoryMock.IsEmailUniqueAsync(email,Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await _handler.Handle(command,CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Conflict);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());

        }
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenNickNameIsNotUnique()
        {
            var email = "correct email";
            var nickName = "duplicate nick name";
            var password = "correct password";
            var command = new RegisterCommand(nickName, email, password);
             _userRepositoryMock.IsNickNameUniqueAsync(nickName, Arg.Any<CancellationToken>())
                .Returns(false);
            _userRepositoryMock.IsEmailUniqueAsync(email, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Conflict);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>(),Arg.Any<CancellationToken>());

        }
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenAllDataIsCorrect()
        {
            var email = "correct email";
            var nickName = "correct nick name";
            var password = "correct password";
            var expectedAcessToken = "acess token";
            var expectedRefreshToken = "refresh token";
            _passwordHasherMock.HashPassword(password)
                .Returns("Hashed password");
            _userRepositoryMock.IsEmailUniqueAsync(email, Arg.Any<CancellationToken>())
                .Returns(true);
            _userRepositoryMock.IsNickNameUniqueAsync(nickName,Arg.Any<CancellationToken>())
                .Returns(true);
            var command = new RegisterCommand(nickName, email, password);
            _jwtTokenGeneratorMock.GenerateRefreshToken()
                .Returns((expectedRefreshToken, DateTime.UtcNow.AddDays(7)));
            _jwtTokenGeneratorMock.GenerateAccessToken(Arg.Any<User>())
                .Returns(expectedAcessToken);

            var result = await _handler.Handle(command, CancellationToken.None);
     
            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(email);
            result.Value.NickName.Should().Be(nickName);
            result.Value.Token.Should().Be(expectedAcessToken);
            result.Value.RefreshToken.Should().Be(expectedRefreshToken);
            result.Value.Id.Should().NotBeEmpty();



            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _userRepositoryMock.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }
        

    }
}
