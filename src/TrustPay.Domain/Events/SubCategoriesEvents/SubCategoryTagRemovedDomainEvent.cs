using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.SubCategoryEvents
{
    public record SubCategoryTagRemovedDomainEvent(
        Guid SubCategoryId,
        Guid TagId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}