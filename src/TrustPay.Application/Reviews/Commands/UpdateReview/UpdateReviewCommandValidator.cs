namespace TrustPay.Application.Reviews.Commands.UpdateReview;

using FluentValidation;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Идентификатор отзыва не может быть пустым.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Заголовок отзыва обязателен.")
            .MaximumLength(200)
            .WithMessage("Заголовок отзыва не должен превышать 200 символов.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Текст отзыва обязателен.")
            .MaximumLength(2000)
            .WithMessage("Текст отзыва не должен превышать 2000 символов.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Оценка должна быть в диапазоне от 1 до 5.");
    }
}