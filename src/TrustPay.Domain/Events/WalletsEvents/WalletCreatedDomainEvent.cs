using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Events.WalletEvents
{
    public record WalletCreatedDomainEvent(
        Guid WalletId,
        Guid UserId,
        Money InitialBalance) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}