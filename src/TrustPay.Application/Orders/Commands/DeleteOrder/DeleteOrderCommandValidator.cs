namespace TrustPay.Application.Orders.Commands.DeleteOrder;

using FluentValidation;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}