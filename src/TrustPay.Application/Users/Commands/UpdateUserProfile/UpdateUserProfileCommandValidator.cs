namespace TrustPay.Application.Users.Commands.UpdateUserProfile;

using FluentValidation;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID пользователя не может быть пустым.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат email.");

        RuleFor(x => x.NickName)
            .NotEmpty().WithMessage("Никнейм обязателен.")
            .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа.")
            .MaximumLength(50).WithMessage("Никнейм не должен превышать 50 символов.");
    }
}