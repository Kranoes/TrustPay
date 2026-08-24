namespace TrustPay.Application.Reviews.Commands.CreateReview;

using FluentValidation;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Идентификатор заказа не может быть пустым.");

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