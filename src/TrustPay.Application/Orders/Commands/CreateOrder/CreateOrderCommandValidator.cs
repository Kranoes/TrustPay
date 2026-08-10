namespace TrustPay.Application.Orders.Commands.CreateOrder;

using FluentValidation;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.ExecutorId)
            .NotEmpty()
            .Must((cmd, executorId) => executorId != cmd.CustomerId)
            .WithMessage("Заказчик и исполнитель не могут быть одним лицом.");

        RuleFor(x => x.LotId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
    }
}