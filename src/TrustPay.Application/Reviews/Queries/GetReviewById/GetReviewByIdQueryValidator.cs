namespace TrustPay.Application.Reviews.Queries.GetById;

using FluentValidation;

public class GetReviewByIdQueryValidator : AbstractValidator<GetReviewByIdQuery>
{
    public GetReviewByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор отзыва не может быть пустым.");
    }
}