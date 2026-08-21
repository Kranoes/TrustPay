namespace TrustPay.Application.Orders.Commands.CreateOrder;

using FluentValidation;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.LotId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        
    }
}