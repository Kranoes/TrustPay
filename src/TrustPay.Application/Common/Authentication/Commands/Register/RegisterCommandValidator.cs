using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace TrustPay.Application.Common.Authentication.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator() 
        {
        RuleFor(x=>x.NickName).NotEmpty().WithMessage("Никнейм не может быть пустым.")
                .MinimumLength(3).WithMessage("Никнейм должен быть не менее 3 символов.")
                .MaximumLength(30).WithMessage("Никнейм не может быть длиннее 30 символов.")
                .Matches(@"^[a-zA-Z0-9_а-яА-ЯёЁ-]+$").WithMessage("Никнейм может содержать только буквы, цифры, дефис и нижнее подчеркивание."); ;
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email не может быть пустым.")
                .EmailAddress().WithMessage("Некорректный формат email.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не может быть пустым.")
            .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов.")
            .Must(password=>password != null && password.Any(char.IsUpper)).WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
            .Must(password => password != null && password.Any(char.IsLower)).WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
            .Must(password => password != null && password.Any(char.IsDigit)).WithMessage("Пароль должен содержать хотя бы одну цифру.")
            .Must(password => password != null && password.Any(ch => !char.IsLetterOrDigit(ch))).WithMessage("Пароль должен содержать хотя бы один специальный символ.");



        }
    }
}
