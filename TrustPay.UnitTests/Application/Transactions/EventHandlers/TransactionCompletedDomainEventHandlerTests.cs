using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Models;
using TrustPay.Application.Common.Transactions.EventHandlers;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Events.TransactionsEvents;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.EventHandlers
{
    public class TransactionCompletedDomainEventHandlerTests
    {
        private readonly IWalletRepository _walletRepositoryMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly TransactionCompletedDomainEventHandler _handler;

        public TransactionCompletedDomainEventHandlerTests()
        {
            _walletRepositoryMock = Substitute.For<IWalletRepository>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new TransactionCompletedDomainEventHandler(
                _walletRepositoryMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenWalletIdIsNull()
        {
            var amount = Money.Create(100, "RUB").Value;
            var domainEvent = new TransactionCompletedDomainEvent(
                Guid.NewGuid(),
                null,
                amount,
                "Card");

            var notification = new DomainEventNotification<TransactionCompletedDomainEvent>(domainEvent);

            await _handler.Handle(notification, CancellationToken.None);

            await _walletRepositoryMock.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenWalletNotFound()
        {
            var walletId = Guid.NewGuid();
            var amount = Money.Create(100, "RUB").Value;
            var domainEvent = new TransactionCompletedDomainEvent(
                Guid.NewGuid(),
                walletId,
                amount,
                "Card");

            var notification = new DomainEventNotification<TransactionCompletedDomainEvent>(domainEvent);

            _walletRepositoryMock.GetByIdAsync(walletId, Arg.Any<CancellationToken>())
                .Returns((Wallet?)null);

            await _handler.Handle(notification, CancellationToken.None);

            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenDepositFails()
        {
            var walletId = Guid.NewGuid();
            var amount = Money.Create(100, "RUB").Value;
            var domainEvent = new TransactionCompletedDomainEvent(
                Guid.NewGuid(),
                walletId,
                amount,
                "Card");

            var notification = new DomainEventNotification<TransactionCompletedDomainEvent>(domainEvent);

            var walletMock = Substitute.For<Wallet>();
            walletMock.Deposit(amount).Returns(Result.Failure(Error.Validation("Wallet.Error", "Ошибка пополнения")));

            _walletRepositoryMock.GetByIdAsync(walletId, Arg.Any<CancellationToken>())
                .Returns(walletMock);

            await _handler.Handle(notification, CancellationToken.None);

            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldDepositAndSaveChanges_WhenWalletExistsAndDepositSucceeds()
        {
            var walletId = Guid.NewGuid();
            var amount = Money.Create(100, "RUB").Value;
            var domainEvent = new TransactionCompletedDomainEvent(
                Guid.NewGuid(),
                walletId,
                amount,
                "Card");

            var notification = new DomainEventNotification<TransactionCompletedDomainEvent>(domainEvent);

            var walletMock = Substitute.For<Wallet>();
            walletMock.Deposit(amount).Returns(Result.Success());

            _walletRepositoryMock.GetByIdAsync(walletId, Arg.Any<CancellationToken>())
                .Returns(walletMock);

            await _handler.Handle(notification, CancellationToken.None);

            walletMock.Received(1).Deposit(amount);
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}