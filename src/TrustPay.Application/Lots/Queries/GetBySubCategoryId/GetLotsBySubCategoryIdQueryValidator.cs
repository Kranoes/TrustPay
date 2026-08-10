namespace TrustPay.Application.Lots.Queries.GetLotsBySubCategoryId;

using FluentValidation;

public class GetLotsBySubCategoryIdQueryValidator : AbstractValidator<GetLotsBySubCategoryIdQuery>
{
    public GetLotsBySubCategoryIdQueryValidator()
    {
        RuleFor(x => x.SubCategoryId)
            .NotEmpty();
    }
}