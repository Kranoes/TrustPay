using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Domain.Events.UserEvents
{
    public record UserRoleChangedDomainEvent(
        Guid UserId,
        UserRole OldRole,
        UserRole NewRole) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}