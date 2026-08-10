using FluentValidation;
using TrustPay.Application.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Указана недопустимая роль пользователя.");
    }
}