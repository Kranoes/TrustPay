using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.DisputeEvents
{
    public record DisputeOpenedDomainEvent(
        Guid DisputeId,
        Guid OrderId,
        Guid CustomerId,
        Guid ExecutorId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}