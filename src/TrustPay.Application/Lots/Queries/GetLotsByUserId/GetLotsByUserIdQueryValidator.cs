namespace TrustPay.Application.Lots.Queries.GetLotsByUserId;

using FluentValidation;

public class GetLotsByUserIdQueryValidator : AbstractValidator<GetLotsByUserIdQuery>
{
    public GetLotsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}