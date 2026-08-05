using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.DisputeEvents
{
    public record DisputeResolvedInFavorOfCustomerDomainEvent(
        Guid DisputeId,
        Guid OrderId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}