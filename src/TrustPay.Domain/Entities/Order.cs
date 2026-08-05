using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.OrderEvents;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Entities
{
    public class Order : AggregateRoot<Guid>
    {
        public Guid CustomerId { get; private set; }
        public Guid ExecutorId { get; private set; }
        public Guid LotId { get; private set; }
        public int Quantity { get; private set; }
        public Money Price { get; private set; } = null!;
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public uint Version { get; private set; }

        public Dispute? Dispute { get; private set; }
        public Review? Review { get; private set; }
        public User Customer { get; private set; } = null!;
        public User Executor { get; private set; } = null!;
        public Lot Lot { get; private set; } = null!;

        private Order() { }

        private Order(
            Guid id,
            Guid customerId,
            Guid executorId,
            Guid lotId,
            int quantity,
            Money price)
            : base(id)
        {
            CustomerId = customerId;
            ExecutorId = executorId;
            LotId = lotId;
            Quantity = quantity;
            Price = price;
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
        }

       
        public static Result<Order> Create(
            Guid customerId,
            Guid executorId,
            Guid lotId,
            int quantity,
            Money price)
        {
            if (customerId == Guid.Empty)
            {
                return Result.Failure<Order>("Идентификатор покупателя не может быть пустым.");
            }

            if (executorId == Guid.Empty)
            {
                return Result.Failure<Order>("Идентификатор исполнителя не может быть пустым.");
            }

            if (customerId == executorId)
            {
                return Result.Failure<Order>("Заказчик и исполнитель не могут быть одним лицом.");
            }

            if (lotId == Guid.Empty)
            {
                return Result.Failure<Order>("Идентификатор лота не может быть пустым.");
            }

            if (quantity <= 0)
            {
                return Result.Failure<Order>("Количество товара должно быть больше нуля.");
            }

            if (price is null)
            {
                return Result.Failure<Order>("Цена должна быть указана.");
            }

            var order = new Order(
                Guid.NewGuid(),
                customerId,
                executorId,
                lotId,
                quantity,
                price);

            order.AddDomainEvent(new OrderCreatedDomainEvent(
                order.Id,
                order.CustomerId,
                order.ExecutorId,
                order.LotId,
                order.Quantity,
                order.Price));

            return Result.Success(order);
        }

        public Result ChangeStatus(OrderStatus newStatus)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            {
                return Result.Failure("Нельзя изменить статус уже завершенного или отмененного заказа.");
            }

            if (Status == newStatus)
            {
                return Result.Success();
            }

            var oldStatus = Status;
            Status = newStatus;

            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, newStatus));

            return Result.Success();
        }

      
        public Result AttachDispute(Dispute dispute)
        {
            if (dispute is null)
            {
                return Result.Failure("Спор не может быть null.");
            }

            if (Dispute is not null)
            {
                return Result.Failure("Для данного заказа спор уже открыт.");
            }

            var oldStatus = Status;
            Dispute = dispute;
            Status = OrderStatus.Disputed;

            AddDomainEvent(new OrderDisputedDomainEvent(Id, dispute.Id));
            AddDomainEvent(new OrderStatusChangedDomainEvent(Id, oldStatus, Status));

            return Result.Success();
        }

        public Result AttachReview(Review review)
        {
            if (review is null)
            {
                return Result.Failure("Отзыв не может быть null.");
            }

            if (Status != OrderStatus.Completed)
            {
                return Result.Failure("Оставить отзыв можно только к завершенному заказу.");
            }

            if (Review is not null)
            {
                return Result.Failure("К этому заказу отзыв уже оставлен.");
            }

            Review = review;

            AddDomainEvent(new OrderReviewAttachedDomainEvent(Id, review.Id));

            return Result.Success();
        }
    }
}