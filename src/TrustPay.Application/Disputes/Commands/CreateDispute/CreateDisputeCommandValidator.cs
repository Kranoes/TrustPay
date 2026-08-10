namespace TrustPay.Application.Disputes.Commands.CreateDispute;

using FluentValidation;

public class CreateDisputeCommandValidator : AbstractValidator<CreateDisputeCommand>
{
    public CreateDisputeCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Идентификатор заказчика не может быть пустым.");

        RuleFor(x => x.ExecutorId)
            .NotEmpty().WithMessage("Идентификатор исполнителя не может быть пустым.");

        RuleFor(x => x)
            .Must(x => x.CustomerId != x.ExecutorId)
            .WithMessage("Заказчик и исполнитель не могут быть одним и тем же пользователем.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Причина спора обязательна для заполнения.")
            .MinimumLength(10).WithMessage("Причина спора должна содержать минимум 10 символов.")
            .MaximumLength(1000).WithMessage("Причина спора не должна превышать 1000 символов.");
    }
}