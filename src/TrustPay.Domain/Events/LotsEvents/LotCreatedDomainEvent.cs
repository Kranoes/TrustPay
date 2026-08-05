using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Events.LotEvents
{
    public record LotCreatedDomainEvent(
        Guid LotId,
        Guid UserId,
        Guid SubCategoryId,
        string Title,
        Money Cost,
        int ItemsCount) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}