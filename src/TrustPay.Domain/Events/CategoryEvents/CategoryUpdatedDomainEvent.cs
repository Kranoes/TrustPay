using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.CategoryEvents
{
    public record CategoryUpdatedDomainEvent(
        Guid CategoryId,
        string Title) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}