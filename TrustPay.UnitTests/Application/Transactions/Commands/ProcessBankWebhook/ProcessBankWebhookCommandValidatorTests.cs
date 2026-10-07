using System;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Transactions.Commands.ProcessBankWebhook;
using Xunit;

namespace TrustPay.UnitTests.Application.Common.Transactions.Commands.ProcessBankWebhook
{
    public class ProcessBankWebhookCommandValidatorTests
    {
        private readonly ProcessBankWebhookCommandValidator _validator = new();

        [Fact]
        public void Should_NotHaveValidationError_WhenSuccessCommandIsValid()
        {
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), true, null, "ext_12345");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveValidationError_WhenFailureCommandIsValid()
        {
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), false, "Недостаточно средств", null);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveValidationError_WhenTransactionIdIsEmpty()
        {
            var command = new ProcessBankWebhookCommand(Guid.Empty, true, null, "ext_12345");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TransactionId)
                .WithErrorMessage("ID транзакции не может быть пустым");
        }

        [Fact]
        public void Should_HaveValidationError_WhenSuccessAndExternalPaymentIdIsEmpty()
        {
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), true, null, "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ExternalPaymentId)
                .WithErrorMessage("При успешной оплате ExternalPaymentId обязателен.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenSuccessAndExternalPaymentIdExceedsMaxLength()
        {
            var longExternalId = new string('a', 101);
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), true, null, longExternalId);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ExternalPaymentId)
                .WithErrorMessage("ExternalPaymentId не может превышать 100 символов.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenFailureAndFailureReasonIsEmpty()
        {
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), false, "", null);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.FailureReason)
                .WithErrorMessage("При неудачной оплате должна быть указана причина.");
        }

        [Fact]
        public void Should_HaveValidationError_WhenFailureAndFailureReasonExceedsMaxLength()
        {
            var longReason = new string('a', 501);
            var command = new ProcessBankWebhookCommand(Guid.NewGuid(), false, longReason, null);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.FailureReason)
                .WithErrorMessage("Причина ошибки не должна превышать 500 символов.");
        }
    }
}