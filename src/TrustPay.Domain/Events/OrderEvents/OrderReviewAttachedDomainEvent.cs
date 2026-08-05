using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.OrderEvents
{
    public record OrderReviewAttachedDomainEvent(
        Guid OrderId,
        Guid ReviewId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}