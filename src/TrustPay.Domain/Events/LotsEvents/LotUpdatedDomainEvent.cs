using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Events.LotEvents
{
    public record LotUpdatedDomainEvent(
        Guid LotId,
        string Title,
        Money Cost) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}