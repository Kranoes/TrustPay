using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.ReviewEvents
{
    public record ReviewCreatedDomainEvent(
        Guid ReviewId,
        Guid OrderId,
        int Rating) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}