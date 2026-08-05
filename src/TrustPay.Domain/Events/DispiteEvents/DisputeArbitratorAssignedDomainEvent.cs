using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.DisputeEvents
{
    public record DisputeArbitratorAssignedDomainEvent(
        Guid DisputeId,
        Guid ArbitratorId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}