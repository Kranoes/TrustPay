using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Authentication.Commands.Logout;

namespace TrustPay.UnitTests.Application.Authentication.Commands.Logout
{
    public class LogoutCommandValidatorTests
    {
        private readonly LogoutCommandValidator _validator = new();
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_ShouldHaveValidationError_WhenRefreshTokenIsEmpty(string? invalidToken)
        {
            var command = new LogoutCommand(invalidToken);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
                .WithErrorMessage("RefreshToken не может быть пустым.");

        }
        [Fact]
        public void Validate_ShouldHaveValidationError_WhenRefreshTokenIsValid()
        {
            var command = new LogoutCommand("valid-token-string");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
