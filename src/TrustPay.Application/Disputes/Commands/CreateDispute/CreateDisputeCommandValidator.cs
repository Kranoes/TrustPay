namespace TrustPay.Application.Disputes.Commands.CreateDispute;

using FluentValidation;

public class CreateDisputeCommandValidator : AbstractValidator<CreateDisputeCommand>
{
    public CreateDisputeCommandValidator()
    {
       
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Причина спора обязательна для заполнения.")
            .MinimumLength(10).WithMessage("Причина спора должна содержать минимум 10 символов.")
            .MaximumLength(1000).WithMessage("Причина спора не должна превышать 1000 символов.");
    }
}