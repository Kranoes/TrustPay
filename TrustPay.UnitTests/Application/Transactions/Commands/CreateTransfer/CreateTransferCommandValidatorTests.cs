using System;
using System.Reflection;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Transactions.Commands.CreateTransfer;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateTransfer
{
    public class CreateTransferCommandValidatorTests
    {
        private readonly CreateTransferCommandValidator _validator = new();

        [Fact]
        public void Should_NotHaveValidationError_WhenCommandIsValid()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveValidationError_WhenSenderWalletIdIsEmpty()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(Guid.Empty, Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.SenderWalletId)
                .WithErrorMessage("Идентификатор отправителя не может быть пустым.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenReceiverWalletIdIsEmpty()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(Guid.NewGuid(), Guid.Empty, money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceiverWalletId)
                .WithErrorMessage("Идентификатор получателя не может быть пустым.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenSenderAndReceiverAreSame()
        {
            var walletId = Guid.NewGuid();
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateTransferCommand(walletId, walletId, money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceiverWalletId)
                .WithErrorMessage("Нельзя перевести средства самому себе.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenAmountIsNull()
        {
            var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), null!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Сумма должна быть указана.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_HaveValidationError_WhenAmountIsZeroOrNegative(decimal amountValue)
        {
            var money = CreateMoneyForTest(amountValue, "RUB");
            var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount.Amount)
                .WithErrorMessage("Сумма перевода должна быть больше нуля.");
        }

        private static Money CreateMoneyForTest(decimal amount, string currency)
        {
            var constructor = typeof(Money).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(decimal), typeof(string) },
                null);

            return (Money)constructor!.Invoke(new object[] { amount, currency });
        }
    }
}