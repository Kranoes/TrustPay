using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.Commands.CreateWithdrawal;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateWithdrawal
{
    public class CreateWithdrawalCommandHandlerTests
    {
        private readonly ITransactionRepository _transactionRepositoryMock;
        private readonly IWalletRepository _walletRepositoryMock;
        private readonly ICurrentUserService _currentUserServiceMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly CreateWithdrawalCommandHandler _handler;

        public CreateWithdrawalCommandHandlerTests()
        {
            _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
            _walletRepositoryMock = Substitute.For<IWalletRepository>();
            _currentUserServiceMock = Substitute.For<ICurrentUserService>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new CreateWithdrawalCommandHandler(
                _transactionRepositoryMock,
                _walletRepositoryMock,
                _currentUserServiceMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenWalletDoesNotExist()
        {
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(Guid.NewGuid(), amount);

            _walletRepositoryMock.GetByIdAsync(command.SenderWalletId, Arg.Any<CancellationToken>())
                .Returns((Wallet?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Wallet.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnForbiddenError_WhenWalletDoesNotBelongToCurrentUser()
        {
            var currentUserId = Guid.NewGuid();
            var alienUserId = Guid.NewGuid();
            var walletId = Guid.NewGuid();

            var wallet = Substitute.For<Wallet>();
            wallet.UserId.Returns(alienUserId);

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(walletId, amount);

            _currentUserServiceMock.UserId.Returns(currentUserId);
            _walletRepositoryMock.GetByIdAsync(walletId, Arg.Any<CancellationToken>())
                .Returns(wallet);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Wallet.AccessDenied");
            result.Error.Type.Should().Be(ErrorType.Forbidden);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCreateWithdrawalTransaction_WhenWalletBelongsToCurrentUserAndDataIsValid()
        {
            var userId = Guid.NewGuid();
            var walletId = Guid.NewGuid();

            var wallet = Substitute.For<Wallet>();
            wallet.UserId.Returns(userId);

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(walletId, amount);

            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(walletId, Arg.Any<CancellationToken>())
                .Returns(wallet);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _transactionRepositoryMock.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}