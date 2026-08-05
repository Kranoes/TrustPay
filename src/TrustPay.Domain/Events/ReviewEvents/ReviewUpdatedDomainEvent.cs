using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.ReviewEvents
{
    public record ReviewUpdatedDomainEvent(
        Guid ReviewId,
        string Title,
        string Message,
        int Rating) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}