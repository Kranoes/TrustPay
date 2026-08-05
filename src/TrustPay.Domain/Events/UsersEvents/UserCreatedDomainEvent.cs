using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Domain.Events.UserEvents
{
    public record UserCreatedDomainEvent(
        Guid UserId,
        string UserEmail,
        string UserName,
        UserRole Role) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}