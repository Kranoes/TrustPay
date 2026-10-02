using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Common.Authentication.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email не может быть пустым.")
                .EmailAddress().WithMessage("Некорректный формат email.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не может быть пустым.");
        }
    }
}
