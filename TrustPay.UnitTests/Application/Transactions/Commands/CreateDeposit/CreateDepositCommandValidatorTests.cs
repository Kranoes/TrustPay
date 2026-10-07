using System;
using System.Reflection;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Transactions.Commands.CreateDeposit;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateDeposit
{
    public class CreateDepositCommandValidatorTests
    {
        private readonly CreateDepositCommandValidator _validator = new();

        [Fact]
        public void Should_NotHaveValidationError_WhenCommandIsValid()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateDepositCommand(Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveValidationError_WhenReceiverWalletIdIsEmpty()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateDepositCommand(Guid.Empty, money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceiverWalletId)
                .WithErrorMessage("Идентификатор кошелька не может быть пустым.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenAmountIsNull()
        {
            var command = new CreateDepositCommand(Guid.NewGuid(), null!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Сумма должна быть указана.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void Should_HaveValidationError_WhenAmountIsZeroOrNegative(decimal amountValue)
        {
            var money = CreateMoneyForTest(amountValue, "RUB");
            var command = new CreateDepositCommand(Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount.Amount)
                .WithErrorMessage("Сумма пополнения должна быть больше нуля.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_HaveValidationError_WhenCurrencyIsEmpty(string? currency)
        {
            var money = CreateMoneyForTest(100, currency!);
            var command = new CreateDepositCommand(Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount.Currency)
                .WithErrorMessage("Валюта должна быть указана.");
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