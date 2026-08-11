namespace TrustPay.Application.Reviews.Commands.DeleteReview;

using FluentValidation;

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор отзыва не может быть пустым.");
    }
}