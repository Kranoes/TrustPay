using FluentValidation.TestHelper;
using TrustPay.Application.Common.Authentication.Commands.Login;

namespace TrustPay.UnitTests.Application.Authentication.Commands.Login
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("plainaddress")]
        [InlineData("user@")]
        [InlineData(".com")]
        public void Validate_ShouldHaveError_WhenEmailIsInvalid(string? invalidEmail)
        {
            var command = new LoginCommand(invalidEmail!, "ValidPassword123%");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldHaveError_WhenPasswordIsInvalid(string? invalidPassword)
        {
            var command = new LoginCommand("valid@email.com", invalidPassword!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
        {
            var command = new LoginCommand("valid@email.com", "ValidPassword123%");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}