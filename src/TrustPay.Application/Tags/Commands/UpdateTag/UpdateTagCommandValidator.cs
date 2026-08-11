namespace TrustPay.Application.Tags.Commands.UpdateTag;

using FluentValidation;

public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор тега обязателен.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Имя тега обязательное поле.")
            .MaximumLength(50)
            .WithMessage("Имя тега не должно превышать 50 символов.");
    }
}