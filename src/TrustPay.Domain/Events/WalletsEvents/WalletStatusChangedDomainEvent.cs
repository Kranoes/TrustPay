using System;
using TrustPay.Domain.Common;
using TrustPay.Domain.Enums;

namespace TrustPay.Domain.Events.WalletEvents
{
    public record WalletStatusChangedDomainEvent(
        Guid WalletId,
        WalletStatus OldStatus,
        WalletStatus NewStatus) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}