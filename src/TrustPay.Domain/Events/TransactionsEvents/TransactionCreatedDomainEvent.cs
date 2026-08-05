using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Common;
using TrustPay.Domain.ValueObjects;

namespace TrustPay.Domain.Events.TransactionsEvents
{
    public record TransactionCreatedDomainEvent(Guid TransactionId,
        Guid? SenderWalletId,
        Guid? ReceiverWalletId,
        Money Amount
        ) : IDomainEvent
    {

        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
