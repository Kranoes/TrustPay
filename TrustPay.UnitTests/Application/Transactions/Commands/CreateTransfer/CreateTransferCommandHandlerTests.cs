using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.Commands.CreateTransfer;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateTransfer
{
    public class CreateTransferCommandHandlerTests
    {
        private readonly ITransactionRepository _transactionRepositoryMock;
        private readonly IWalletRepository _walletRepositoryMock;
        private readonly ICurrentUserService _currentUserServiceMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly CreateTransferCommandHandler _handler;

        public CreateTransferCommandHandlerTests()
        {
            _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
            _walletRepositoryMock = Substitute.For<IWalletRepository>();
            _currentUserServiceMock = Substitute.For<ICurrentUserService>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new CreateTransferCommandHandler(
                _transactionRepositoryMock,
                _walletRepositoryMock,
                _currentUserServiceMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationError_WhenSenderAndReceiverWalletsAreSame()
        {
            var sameWalletId = Guid.NewGuid();
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(sameWalletId, sameWalletId, amount);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Transfer.SameWallet");
            result.Error.Type.Should().Be(ErrorType.Validation);
            await _walletRepositoryMock.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenSenderWalletDoesNotExist()
        {
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), amount);

            _walletRepositoryMock.GetByIdAsync(command.SenderWalletId, Arg.Any<CancellationToken>())
                .Returns((Wallet?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Wallet.SenderNotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnForbiddenError_WhenSenderWalletDoesNotBelongToCurrentUser()
        {
            var currentUserId = Guid.NewGuid();
            var alienUserId = Guid.NewGuid();
            var senderWalletId = Guid.NewGuid();
            var receiverWalletId = Guid.NewGuid();

            var senderWallet = Substitute.For<Wallet>();
            senderWallet.UserId.Returns(alienUserId);

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(senderWalletId, receiverWalletId, amount);

            _currentUserServiceMock.UserId.Returns(currentUserId);
            _walletRepositoryMock.GetByIdAsync(senderWalletId, Arg.Any<CancellationToken>())
                .Returns(senderWallet);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Wallet.AccessDenied");
            result.Error.Type.Should().Be(ErrorType.Forbidden);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenReceiverWalletDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var senderWallet = Substitute.For<Wallet>();
            senderWallet.UserId.Returns(userId);

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(senderId, receiverId, amount);

            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
                .Returns(senderWallet);

            _walletRepositoryMock.GetByIdAsync(receiverId, Arg.Any<CancellationToken>())
                .Returns((Wallet?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Wallet.ReceiverNotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCreateTransferTransaction_WhenSenderBelongsToUserAndReceiverExists()
        {
            var userId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var senderWallet = Substitute.For<Wallet>();
            senderWallet.UserId.Returns(userId);

            var receiverWallet = Substitute.For<Wallet>();

            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(senderId, receiverId, amount);

            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
                .Returns(senderWallet);
            _walletRepositoryMock.GetByIdAsync(receiverId, Arg.Any<CancellationToken>())
                .Returns(receiverWallet);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _transactionRepositoryMock.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}