namespace TrustPay.Domain.Events.DisputeEvents;

using System;
using TrustPay.Domain.Common;

public record DisputeCancelledDomainEvent(
    Guid DisputeId,
    Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}