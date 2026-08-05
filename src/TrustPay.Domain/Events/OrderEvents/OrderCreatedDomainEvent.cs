using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Events.OrderEvents
{
    public record OrderCreatedDomainEvent(
        Guid OrderId,
        Guid CustomerId,
        Guid ExecutorId,
        Guid LotId,
        int Quantity,
        Money Price) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}