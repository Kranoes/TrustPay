namespace TrustPay.Application.Disputes.Queries.GetDisputes;

using FluentValidation;

public class GetDisputesQueryValidator : AbstractValidator<GetDisputesQuery>
{
    public GetDisputesQueryValidator()
    {
        When(x => x.Keywords != null, () =>
        {
            RuleFor(x => x.Keywords)
                .Must(k => k!.Length > 0).WithMessage("Массив ключевых слов не должен быть пустым, если передан.")
                .Must(k => k!.Length <= 5).WithMessage("Нельзя передавать более 5 ключевых слов за один запрос.");

            RuleForEach(x => x.Keywords)
                .NotEmpty().WithMessage("Ключевое слово не может быть пустым.")
                .MaximumLength(50).WithMessage("Длина ключевого слова не должна превышать 50 символов.");
        });
    }
}