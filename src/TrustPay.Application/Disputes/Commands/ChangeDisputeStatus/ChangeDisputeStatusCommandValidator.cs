namespace TrustPay.Application.Disputes.Commands.ChangeDisputeStatus;

using FluentValidation;

public class ChangeDisputeStatusCommandValidator : AbstractValidator<ChangeDisputeStatusCommand>
{
    public ChangeDisputeStatusCommandValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty()
            .WithMessage("Идентификатор спора не может быть пустым GUID.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Передан невалидный статус спора.");
    }
}