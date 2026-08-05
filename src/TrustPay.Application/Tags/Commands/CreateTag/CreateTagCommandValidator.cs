using FluentValidation;

namespace TrustPay.Application.Tags.Commands.CreateTag
{
    public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
    {
        public CreateTagCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя тега не может быть пустым.")
                .MinimumLength(2).WithMessage("Минимальная длина тега — 2 символа.")
                .MaximumLength(50).WithMessage("Максимальная длина тега — 50 символов.")
                .Matches(@"^[\p{L}0-9\s\-_]+$")
                .WithMessage("Имя тега может содержать только буквы, цифры, пробелы, дефисы и нижние подчеркивания.");
        }
    }
}