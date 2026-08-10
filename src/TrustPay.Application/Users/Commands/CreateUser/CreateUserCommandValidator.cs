using FluentValidation;
using TrustPay.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.");

        RuleFor(x => x.NickName)
            .NotEmpty().WithMessage("Никнейм обязателен.")
            .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Указана недопустимая роль пользователя.");
    }
}