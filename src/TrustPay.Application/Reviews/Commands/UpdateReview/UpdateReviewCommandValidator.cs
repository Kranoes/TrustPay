namespace TrustPay.Application.Reviews.Commands.UpdateReview;

using FluentValidation;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор отзыва обязателен.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Заголовок отзыва обязателен.")
            .MaximumLength(50)
            .WithMessage("Заголовок отзыва не должен превышать 50 символов.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Текст отзыва обязателен.")
            .MaximumLength(200)
            .WithMessage("Текст отзыва не должен превышать 200 символов.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Оценка должна быть в диапазоне от 1 до 5.");
    }
}