using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.OrderEvents
{
    public record OrderDisputedDomainEvent(
        Guid OrderId,
        Guid DisputeId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}