namespace TrustPay.Application.Orders.Commands.StartOrder;

using FluentValidation;

public class StartOrderCommandValidator : AbstractValidator<StartOrderCommand>
{
    public StartOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Идентификатор заказа обязателен.");
    }
}