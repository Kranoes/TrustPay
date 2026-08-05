using FluentValidation;

namespace TrustPay.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок категории обязателен.")
            .MaximumLength(100).WithMessage("Заголовок не должен превышать 100 символов.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не должно превышать 500 символов.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Указан неверный тип категории.");
    }
}