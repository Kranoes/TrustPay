using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Domain.Events.CategoryEvents
{
    public record CategoryCreatedDomainEvent(
        Guid CategoryId,
        string Title,
        CategoryType Type) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}