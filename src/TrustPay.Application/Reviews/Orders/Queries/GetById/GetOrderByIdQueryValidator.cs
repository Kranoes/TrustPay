namespace TrustPay.Application.Orders.Queries.GetById;

using FluentValidation;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}