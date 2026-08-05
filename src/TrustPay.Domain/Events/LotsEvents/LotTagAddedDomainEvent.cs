using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.LotEvents
{
    public record LotTagAddedDomainEvent(
        Guid LotId,
        Guid TagId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}