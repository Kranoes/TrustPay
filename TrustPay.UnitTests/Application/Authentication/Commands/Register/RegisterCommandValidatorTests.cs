using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TrustPay.Application.Common.Authentication.Commands.Register;

namespace TrustPay.UnitTests.Application.Authentication.Commands.Register
{
    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _validator = new();
        [Theory]
        [InlineData("us")]
        [InlineData("superMegaLongCoolNickNAME22222000004")]
        [InlineData("nick@name")]
        [InlineData(" ")]
        public void Validate_ShouldHaveError_WhenNickNameIsInvalid(string? invalidNickName)
        {
            var command = new RegisterCommand(invalidNickName,"valid@email.com", "Password123%@@");
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.NickName);
        }
        [Theory]
        [InlineData("")]
        [InlineData("user@.com@")]
        [InlineData("againnonvalid")]
        [InlineData("@mail.com")]
        [InlineData("trytovalidateme@")]
        public void Validate_ShouldHaveError_WhenEmailIsInvalid(string? invalidEmail)
        {
            var command = new RegisterCommand("ValidNicknamee2", invalidEmail, "Password1234%");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x=>x.Email);
        }
        [Theory]
        [InlineData("", "Пароль не может быть пустым.")]
        [InlineData("Short1!", "Пароль должен быть не менее 8 символов.")]
        [InlineData("lowercase123!", "Пароль должен содержать хотя бы одну заглавную букву.")]
        [InlineData("UPPERCASE123!", "Пароль должен содержать хотя бы одну строчную букву.")]
        [InlineData("NoDigitsHere!", "Пароль должен содержать хотя бы одну цифру.")]
        public void Validate_ShouldHaveError_WhenPasswordIsInvalid(string invalidPassword, string expectedErrorMessage)
        {
            var command = new RegisterCommand("valid_nick", "valid@email.com", invalidPassword);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage(expectedErrorMessage);
        }
        [Fact]
        public void Validate_ShouldHaveSuccess_WhenAllDataIsValid()
        {
            string validNickName = "ValidNickName";
            string validEmail = "valid@email.com";
            string validPassword = "validPassword5%";
            var command = new RegisterCommand(validNickName,validEmail,validPassword);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
