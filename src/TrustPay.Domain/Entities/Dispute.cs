using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;
using TrustPay.Domain.Events.DisputeEvents;

namespace TrustPay.Domain.Entities
{
    public class Dispute : AggregateRoot<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid ExecutorId { get; private set; }
        public Guid? ArbitratorId { get; private set; }
        public DisputeStatus Status { get; private set; }
        public string Reason { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }

        public Order Order { get; private set; } = null!;

        private Dispute() { }

        private Dispute(
            Guid id,
            Guid orderId,
            Guid customerId,
            Guid executorId,
            string reason)
            : base(id)
        {
            OrderId = orderId;
            CustomerId = customerId;
            ExecutorId = executorId;
            Reason = reason;
            ArbitratorId = null;
            Status = DisputeStatus.Opened;
            CreatedAt = DateTime.UtcNow;
        }

        
        public static Result<Dispute> Create(Guid orderId, Guid customerId, Guid executorId, string reason)
        {
            if (orderId == Guid.Empty)
            {
                return Result.Failure<Dispute>("Идентификатор заказа не может быть пустым.");
            }

            if (customerId == Guid.Empty || executorId == Guid.Empty)
            {
                return Result.Failure<Dispute>("Идентификаторы участников должны быть указаны.");
            }

            if (customerId == executorId)
            {
                return Result.Failure<Dispute>("Заказчик и исполнитель не могут быть одним человеком.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Result.Failure<Dispute>("Нельзя создать спор без причины.");
            }

            var dispute = new Dispute(
                Guid.NewGuid(),
                orderId,
                customerId,
                executorId,
                reason.Trim());

            dispute.AddDomainEvent(new DisputeOpenedDomainEvent(
                dispute.Id,
                dispute.OrderId,
                dispute.CustomerId,
                dispute.ExecutorId));

            return Result.Success(dispute);
        }

        
        public Result AssignArbitrator(Guid arbitratorId)
        {
            if (arbitratorId == Guid.Empty)
            {
                return Result.Failure("Указан некорректный ID арбитра.");
            }

            if (Status != DisputeStatus.Opened)
            {
                return Result.Failure("Назначить арбитра можно только для спора в статусе Opened.");
            }

            ArbitratorId = arbitratorId;
            Status = DisputeStatus.UnderReview;

            AddDomainEvent(new DisputeArbitratorAssignedDomainEvent(Id, arbitratorId));

            return Result.Success();
        }

       
        public Result ResolveInFavorOfCustomer()
        {
            if (Status != DisputeStatus.UnderReview)
            {
                return Result.Failure("Решение по спору может быть вынесено только во время рассмотрения.");
            }

            Status = DisputeStatus.ResolvedForBuyer;

            AddDomainEvent(new DisputeResolvedInFavorOfCustomerDomainEvent(Id, OrderId));

            return Result.Success();
        }
        public Result Cancel()
        {
            if (Status is DisputeStatus.ResolvedForBuyer or DisputeStatus.ResolvedForSeller or DisputeStatus.Cancelled)
            {
                return Result.Failure("Нельзя отменить уже закрытый или ранее отмененный спор.");
            }

            Status = DisputeStatus.Cancelled;

            AddDomainEvent(new DisputeCancelledDomainEvent(Id, OrderId));

            return Result.Success();
        }

        public Result ResolveInFavorOfExecutor()
        {
            if (Status != DisputeStatus.UnderReview)
            {
                return Result.Failure("Решение по спору может быть вынесено только во время рассмотрения.");
            }

            Status = DisputeStatus.ResolvedForSeller;

            AddDomainEvent(new DisputeResolvedInFavorOfExecutorDomainEvent(Id, OrderId));

            return Result.Success();
        }
    }
}