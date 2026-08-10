namespace TrustPay.Application.Orders.Commands.UpdateOrderStatus;

using FluentValidation;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.NewStatus)
            .IsInEnum();
    }
}