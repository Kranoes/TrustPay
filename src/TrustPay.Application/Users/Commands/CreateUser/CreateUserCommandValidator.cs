using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Пустой email недопустим.")
                .MaximumLength(100)
                .WithMessage("Максимальная длина почты 100 символов.")
                .EmailAddress()
                .WithMessage("Некорректный формат email");
            RuleFor(x => x.NickName)
                .NotEmpty()
                .WithMessage("Ник не может быть пустой.")
                .MaximumLength(50)
                .WithMessage("Максимальная длина ника 50 символов.");
        }
    }
}
