using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.SubCategoryEvents
{
    public record SubCategoryTitleUpdatedDomainEvent(
        Guid SubCategoryId,
        string NewTitle) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}