using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.SubCategoryEvents
{
    public record SubCategoryCreatedDomainEvent(
        Guid SubCategoryId,
        Guid CategoryId,
        string Title) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}