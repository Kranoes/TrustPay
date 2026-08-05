using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.LotEvents
{
    public record LotSubCategoryChangedDomainEvent(
        Guid LotId,
        Guid NewSubCategoryId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}