namespace TrustPay.Application.Lots.Commands.DeleteLot;

using FluentValidation;

public class DeleteLotCommandValidator : AbstractValidator<DeleteLotCommand>
{
    public DeleteLotCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}