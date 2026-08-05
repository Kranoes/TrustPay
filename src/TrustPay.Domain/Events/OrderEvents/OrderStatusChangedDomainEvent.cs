using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Domain.Events.OrderEvents
{
    public record OrderStatusChangedDomainEvent(
        Guid OrderId,
        OrderStatus OldStatus,
        OrderStatus NewStatus) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}