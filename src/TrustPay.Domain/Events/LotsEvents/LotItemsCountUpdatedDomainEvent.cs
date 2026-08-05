using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.LotEvents
{
    public record LotItemsCountUpdatedDomainEvent(
        Guid LotId,
        int NewCount) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}