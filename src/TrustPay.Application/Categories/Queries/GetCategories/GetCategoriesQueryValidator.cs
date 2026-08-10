namespace TrustPay.Application.Categories.Queries.GetCategories;

using FluentValidation;

public class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesQueryValidator()
    {
        When(x => x.SubCategoryId.HasValue, () =>
        {
            RuleFor(x => x.SubCategoryId)
                .NotEmpty()
                .WithMessage("Идентификатор подкатегории не может быть пустым GUID.");
        });

        When(x => x.Keywords != null, () =>
        {
            RuleFor(x => x.Keywords)
                .Must(k => k!.Length > 0)
                .WithMessage("Массив ключевых слов не может быть пустым, если он передан.")
                .Must(k => k!.Length <= 10)
                .WithMessage("Нельзя передавать более 10 ключевых слов за один запрос.");

            RuleForEach(x => x.Keywords)
                .NotEmpty()
                .WithMessage("Ключевое слово не может быть пустым или состоять только из пробелов.")
                .MaximumLength(50)
                .WithMessage("Длина ключевого слова не должна превышать 50 символов.");
        });
    }
}