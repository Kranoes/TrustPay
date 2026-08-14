using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Common.Authentication.Queries.Login
{
    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email не может быть пустым.")
                .EmailAddress().WithMessage("Некорректный формат email.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не может быть пустым.");
        }
    }
}
