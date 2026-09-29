namespace TrustPay.UnitTests.Application.Authentication.Commands.Register;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Authentication.Commands.Register;
using TrustPay.Application.Common.Authentication.DTOs;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.BloomFilter;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
using Xunit;

public class RegisterCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserValidationService _userValidationServiceMock;
    private readonly IJwtTokenGenerator _jwtTokenGeneratorMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userValidationServiceMock = Substitute.For<IUserValidationService>();
        _jwtTokenGeneratorMock = Substitute.For<IJwtTokenGenerator>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();

        _handler = new RegisterCommandHandler(
            _unitOfWorkMock,
            _userRepositoryMock,
            _jwtTokenGeneratorMock,
            _passwordHasherMock,
            _userValidationServiceMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsTaken()
    {
        var email = "duplicate@email.com";
        var nickName = "correct_nickname";
        var password = "correct_password";
        var command = new RegisterCommand(nickName, email, password);

        _userValidationServiceMock.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNickNameIsTaken()
    {
        var email = "correct@email.com";
        var nickName = "duplicate_nickname";
        var password = "correct_password";
        var command = new RegisterCommand(nickName, email, password);

        _userValidationServiceMock.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(false);
        _userValidationServiceMock.IsNickNameTakenAsync(nickName, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _userRepositoryMock.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllDataIsCorrect()
    {
        var email = "correct@email.com";
        var nickName = "correct_nickname";
        var password = "correct_password";
        var expectedAccessToken = "access_token";
        var expectedRefreshToken = "refresh_token";

        var command = new RegisterCommand(nickName, email, password);

        _userValidationServiceMock.IsEmailTakenAsync(email, Arg.Any<CancellationToken>())
            .Returns(false);
        _userValidationServiceMock.IsNickNameTakenAsync(nickName, Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasherMock.HashPassword(password)
            .Returns("Hashed_password");

        _jwtTokenGeneratorMock.GenerateRefreshToken()
            .Returns((expectedRefreshToken, DateTime.UtcNow.AddDays(7)));
        _jwtTokenGeneratorMock.GenerateAccessToken(Arg.Any<User>())
            .Returns(expectedAccessToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
        result.Value.NickName.Should().Be(nickName);
        result.Value.Token.Should().Be(expectedAccessToken);
        result.Value.RefreshToken.Should().Be(expectedRefreshToken);
        result.Value.Id.Should().NotBeEmpty();

        await _userRepositoryMock.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _userValidationServiceMock.Received(1).RegisterUserEmailAsync(email, Arg.Any<CancellationToken>());
        await _userValidationServiceMock.Received(1).RegisterUserNickNameAsync(nickName, Arg.Any<CancellationToken>());
    }
}