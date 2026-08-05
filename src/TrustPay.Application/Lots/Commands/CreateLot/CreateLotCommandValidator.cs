using FluentValidation;

namespace TrustPay.Application.Lots.Commands.CreateLot;

public class CreateLotCommandValidator : AbstractValidator<CreateLotCommand>
{
    public CreateLotCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя не может быть пустым.");

        RuleFor(x => x.SubCategoryId)
            .NotEmpty().WithMessage("Идентификатор подкатегории не может быть пустым.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок лота не может быть пустым.")
            .MaximumLength(200).WithMessage("Заголовок не должен превышать 200 символов.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Стоимость должна быть больше нуля.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Валюта должна быть указана.")
            .Length(3).WithMessage("Код валюты должен состоять из 3 символов.");

        RuleFor(x => x.ItemsCount)
            .GreaterThanOrEqualTo(0).WithMessage("Количество товаров не может быть отрицательным.");
    }
}