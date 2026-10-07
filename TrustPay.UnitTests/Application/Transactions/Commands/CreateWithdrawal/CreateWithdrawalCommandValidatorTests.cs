using System;
using System.Reflection;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Transactions.Commands.CreateWithdrawal;
using TrustPay.Domain.ValueObjects;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.CreateWithdrawal
{
    public class CreateWithdrawalCommandValidatorTests
    {
        private readonly CreateWithdrawalCommandValidator _validator = new();

        [Fact]
        public void Should_NotHaveValidationError_WhenCommandIsValid()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveValidationError_WhenSenderWalletIdIsEmpty()
        {
            var money = Money.Create(100, "RUB").Value;
            var command = new CreateWithdrawalCommand(Guid.Empty, money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.SenderWalletId)
                .WithErrorMessage("Идентификатор кошелька отправителя не может быть пустым.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenAmountIsNull()
        {
            var command = new CreateWithdrawalCommand(Guid.NewGuid(), null!);

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
            var command = new CreateWithdrawalCommand(Guid.NewGuid(), money);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Amount.Amount)
                .WithErrorMessage("Сумма вывода должна быть больше нуля.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_HaveValidationError_WhenCurrencyIsEmpty(string? currency)
        {
            var money = CreateMoneyForTest(100, currency!);
            var command = new CreateWithdrawalCommand(Guid.NewGuid(), money);

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