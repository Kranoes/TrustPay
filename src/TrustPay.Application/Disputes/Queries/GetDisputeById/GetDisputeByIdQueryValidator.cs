namespace TrustPay.Application.Disputes.Queries.GetDisputeById;

using FluentValidation;

public class GetDisputeByIdQueryValidator : AbstractValidator<GetDisputeByIdQuery>
{
    public GetDisputeByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор спора не может быть пустым GUID.");
    }
}