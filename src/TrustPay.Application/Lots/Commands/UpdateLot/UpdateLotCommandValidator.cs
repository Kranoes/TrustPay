namespace TrustPay.Application.Lots.Commands.UpdateLot;

using FluentValidation;

public class UpdateLotCommandValidator : AbstractValidator<UpdateLotCommand>
{
    public UpdateLotCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.ItemsCount)
            .GreaterThanOrEqualTo(0);
    }
}