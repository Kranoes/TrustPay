namespace TrustPay.Application.Orders.Commands.CancelOrder;

using FluentValidation;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Идентификатор заказа обязателен.");
    }
}