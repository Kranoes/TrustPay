using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.OrderEvents
{
    public record OrderCancelledDomainEvent(Guid OrderId, Guid CancelledByUserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;

    }
}
