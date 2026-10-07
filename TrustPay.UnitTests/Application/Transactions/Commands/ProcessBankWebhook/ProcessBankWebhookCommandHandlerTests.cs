using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.Commands.ProcessBankWebhook;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.Enums;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.ProcessBankWebhook
{
    public class ProcessBankWebhookCommandHandlerTests
    {
        private readonly ITransactionRepository _transactionRepositoryMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly ProcessBankWebhookCommandHandler _handler;

        public ProcessBankWebhookCommandHandlerTests()
        {
            _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new ProcessBankWebhookCommandHandler(
                _transactionRepositoryMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenTransactionDoesNotExist()
        {
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), true, null, "ext_12345");

            _transactionRepositoryMock.GetByIdAsync(command.TransactionId, Arg.Any<CancellationToken>())
                .Returns((Transaction?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Transaction.NotFound");
            result.Error.Type.Should().Be(ErrorType.NotFound);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessWithoutSaving_WhenTransactionIsAlreadyCompleted()
        {
            var transaction = CreateTransactionWithStatus(TransactionStatus.Completed);
            var command = new ProcessBankWebhookCommand(transaction.Id, true, null, "ext_12345");

            _transactionRepositoryMock.GetByIdAsync(command.TransactionId, Arg.Any<CancellationToken>())
                .Returns(transaction);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnConflictError_WhenTransactionIsAlreadyFailed()
        {
            var transaction = CreateTransactionWithStatus(TransactionStatus.Failed);
            var command = new ProcessBankWebhookCommand(transaction.Id, true, null, "ext_12345");

            _transactionRepositoryMock.GetByIdAsync(command.TransactionId, Arg.Any<CancellationToken>())
                .Returns(transaction);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Transaction.InvalidStatus");
            result.Error.Type.Should().Be(ErrorType.Conflict);
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCompleteTransactionAndSave_WhenWebhookIsSuccess()
        {
            var transaction = CreateTransactionWithStatus(TransactionStatus.Pending);
            var command = new ProcessBankWebhookCommand(transaction.Id, true, null, "ext_12345");

            _transactionRepositoryMock.GetByIdAsync(command.TransactionId, Arg.Any<CancellationToken>())
                .Returns(transaction);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(TransactionStatus.Completed);
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldFailTransactionAndSave_WhenWebhookIsFailure()
        {
            var transaction = CreateTransactionWithStatus(TransactionStatus.Pending);
            var command = new ProcessBankWebhookCommand(transaction.Id, false, "Card limit exceeded", null);

            _transactionRepositoryMock.GetByIdAsync(command.TransactionId, Arg.Any<CancellationToken>())
                .Returns(transaction);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            transaction.Status.Should().Be(TransactionStatus.Failed);
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        private static Transaction CreateTransactionWithStatus(TransactionStatus status)
        {
            var amount = Money.Create(100, "RUB").Value;
            var transaction = Transaction.CreateDeposit(Guid.NewGuid(), amount).Value;

            var property = typeof(Transaction).GetProperty(nameof(Transaction.Status));
            property!.SetValue(transaction, status);

            return transaction;
        }
    }
}