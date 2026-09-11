namespace TrustPay.Application.Common.Authentication.Commands.Logout;

using FluentValidation;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("RefreshToken не может быть пустым.");
    }
}