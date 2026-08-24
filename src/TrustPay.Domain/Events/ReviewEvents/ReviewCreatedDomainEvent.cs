namespace TrustPay.Domain.Events.ReviewEvents;

using System;
using TrustPay.Domain.Common;

public record ReviewCreatedDomainEvent(
    Guid ReviewId,
    Guid OrderId,
    Guid AuthorId,
    Guid TargetUserId,
    int Rating) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}