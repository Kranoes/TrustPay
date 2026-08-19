using FluentValidation;
using TrustPay.Application.SubCategories.Commands.CreateSubCategory;

public class CreateSubCategoryCommandValidator : AbstractValidator<CreateSubCategoryCommand>
{
    public CreateSubCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Идентификатор категории не может быть пустым.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок подкатегории не может быть пустым.")
            .MaximumLength(100).WithMessage("Заголовок не должен превышать 100 символов.");
    }
}