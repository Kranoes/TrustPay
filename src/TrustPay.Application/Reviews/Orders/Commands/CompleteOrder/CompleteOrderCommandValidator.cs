namespace TrustPay.Application.Orders.Commands.CompleteOrder;

using FluentValidation;

public class CompleteOrderCommandValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Идентификатор заказа обязателен.");
    }
}