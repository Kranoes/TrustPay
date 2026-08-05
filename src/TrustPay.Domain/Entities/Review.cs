using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Events.ReviewEvents;

namespace TrustPay.Domain.Entities
{
    public class Review : AggregateRoot<Guid>
    {
        public Guid OrderId { get; private set; }
        public string Title { get; private set; } = null!;
        public string Message { get; private set; } = null!;
        public int Rating { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Order Order { get; private set; } = null!;

        private Review() { }

        private Review(Guid id, Guid orderId, string title, string message, int rating)
            : base(id)
        {
            OrderId = orderId;
            Title = title;
            Message = message;
            Rating = rating;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Review> Create(Guid orderId, string title, string message, int rating)
        {
            if (orderId == Guid.Empty)
            {
                return Result.Failure<Review>("Идентификатор заказа не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Result.Failure<Review>("Заголовок отзыва не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return Result.Failure<Review>("Текст отзыва не может быть пустым.");
            }

            if (rating is < 1 or > 5)
            {
                return Result.Failure<Review>("Оценка должна быть в диапазоне от 1 до 5.");
            }

            var review = new Review(
                Guid.NewGuid(),
                orderId,
                title.Trim(),
                message.Trim(),
                rating);

            review.AddDomainEvent(new ReviewCreatedDomainEvent(
                review.Id,
                review.OrderId,
                review.Rating));

            return Result.Success(review);
        }

        public Result Update(string newTitle, string newMessage, int newRating)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return Result.Failure("Заголовок отзыва не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(newMessage))
            {
                return Result.Failure("Текст отзыва не может быть пустым.");
            }

            if (newRating is < 1 or > 5)
            {
                return Result.Failure("Оценка должна быть в диапазоне от 1 до 5.");
            }

            Title = newTitle.Trim();
            Message = newMessage.Trim();
            Rating = newRating;

            AddDomainEvent(new ReviewUpdatedDomainEvent(Id, Title, Message, Rating));

            return Result.Success();
        }
    }
}