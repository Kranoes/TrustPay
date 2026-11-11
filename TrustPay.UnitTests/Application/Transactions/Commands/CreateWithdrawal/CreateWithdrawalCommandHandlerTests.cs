using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
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
            var walletResult = Wallet.Create(alienUserId,Money.Zero("RUB").Value);
            walletResult.IsSuccess.Should().BeTrue();
            var wallet = walletResult.Value;

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(wallet.Id, amount);

            _currentUserServiceMock.UserId.Returns(currentUserId);
            _walletRepositoryMock.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
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
            var walletResult = Wallet.Create(userId,Money.Zero("RUB").Value);
            walletResult.IsSuccess.Should().BeTrue();
            var wallet = walletResult.Value;
            Transaction? transaction = null;
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(wallet.Id, amount);
            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
                .Returns(wallet);

            _transactionRepositoryMock.AddAsync(Arg.Do<Transaction>(t=> transaction = t),Arg.Any<CancellationToken>());
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            transaction.Should().NotBeNull();
            transaction.Type.Should().Be(TrustPay.Domain.Enums.TransactionType.Withdrawal);
            transaction.Amount.Should().Be(amount);
            transaction.ReceiverWalletId.Should().BeNull();
            transaction.SenderWalletId.Should().Be(wallet.Id);
            await _transactionRepositoryMock.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}