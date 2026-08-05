using System;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.UserEvents
{
    public record UserWalletAssignedDomainEvent(
        Guid UserId,
        Guid WalletId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}