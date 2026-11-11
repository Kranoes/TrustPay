using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Interfaces.Auth;
using TrustPay.Application.Common.Interfaces.EntitiesRepo;
using TrustPay.Application.Common.Transactions.Commands.CreateDeposit;
using TrustPay.Application.Common.Transactions.DTOs;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateDeposit
{
    public class CreateDepositCommandHandlerTests
    {
        private readonly ITransactionRepository _transactionRepositoryMock;
        private readonly IPaymentGatewayService _paymentGatewayServiceMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly IWalletRepository _walletRepositoryMock;
        private readonly ICurrentUserService _currentUserServiceMock;
        private readonly CreateDepositCommandHandler _handler;

        public CreateDepositCommandHandlerTests()
        {
            _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
            _paymentGatewayServiceMock = Substitute.For<IPaymentGatewayService>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _walletRepositoryMock = Substitute.For<IWalletRepository>();
            _currentUserServiceMock = Substitute.For<ICurrentUserService>();

            _handler = new CreateDepositCommandHandler(
                _transactionRepositoryMock,
                _unitOfWorkMock,
                _paymentGatewayServiceMock,
                _walletRepositoryMock,
                _currentUserServiceMock);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundError_WhenWalletDoesNotExist()
        {
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateDepositCommand(Guid.NewGuid(), amount);

            _walletRepositoryMock.GetByIdAsync(command.ReceiverWalletId, Arg.Any<CancellationToken>())
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
            var command = new CreateDepositCommand(wallet.Id, amount);

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
        public async Task Handle_ShouldReturnValidationError_WhenTransactionCreationFails()
        {
            var userId = Guid.NewGuid();
            var walletResult = Wallet.Create(userId, Money.Zero("RUB").Value);
            walletResult.IsSuccess.Should().BeTrue();
            var wallet = walletResult.Value;
            var zeroAmount = Money.Create(0, "RUB").Value;
            var command = new CreateDepositCommand(wallet.Id, zeroAmount);
            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
                .Returns(wallet);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Validation);
            await _transactionRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnFailureErrorAndMarkTransactionFailed_WhenPaymentGatewayThrowsException()
        {
            var userId = Guid.NewGuid();
            var walletResult = Wallet.Create(userId,Money.Zero("RUB").Value);
            walletResult.IsSuccess.Should().BeTrue();
            var wallet = walletResult.Value; 
            var amount = Money.Create(100, "RUB").Value;
            Transaction? transaction = null;
            var command = new CreateDepositCommand(wallet.Id, amount);
            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
                .Returns(wallet);
            _paymentGatewayServiceMock.CreatePaymentFormAsync(Arg.Any<Guid>(), Arg.Any<Money>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Gateway unavailable"));

            _transactionRepositoryMock.AddAsync(Arg.Do<Transaction>(t=>transaction = t),Arg.Any<CancellationToken>());
            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            transaction.Status.Should().Be(TrustPay.Domain.Enums.TransactionStatus.Failed);
            result.Error.Code.Should().Be("PaymentGateway.Error");
            result.Error.Type.Should().Be(ErrorType.Failure);
            await _unitOfWorkMock.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldCreateDepositAndReturnPaymentUrl_WhenSuccessful()
        {
            var userId = Guid.NewGuid();
            var walletResult = Wallet.Create(userId,Money.Zero("RUB").Value);
            walletResult.IsSuccess.Should().BeTrue();
            var wallet = walletResult.Value;
            var amount = Money.Create(100, "RUB").Value;
            var command = new CreateDepositCommand(wallet.Id, amount);
            _currentUserServiceMock.UserId.Returns(userId);
            _walletRepositoryMock.GetByIdAsync(wallet.Id, Arg.Any<CancellationToken>())
                .Returns(wallet);
            _paymentGatewayServiceMock.CreatePaymentFormAsync(Arg.Any<Guid>(), Arg.Any<Money>(), Arg.Any<CancellationToken>())
                .Returns(new PaymentGatewayResult("https://mok-bank.com/checkout/123", "ext_123"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.PaymentUrl.Should().Be("https://mok-bank.com/checkout/123");
            await _transactionRepositoryMock.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _unitOfWorkMock.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}