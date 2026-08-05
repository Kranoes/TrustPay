using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.TagEvents
{
    public record TagCreatedDomainEvent(
        Guid TagId,
        string Name) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}