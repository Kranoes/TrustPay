namespace TrustPay.Application.Reviews.Queries.GetByOrderId;

using FluentValidation;

public class GetReviewByOrderIdQueryValidator : AbstractValidator<GetReviewByOrderIdQuery>
{
    public GetReviewByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Идентификатор заказа не может быть пустым.");
    }
}