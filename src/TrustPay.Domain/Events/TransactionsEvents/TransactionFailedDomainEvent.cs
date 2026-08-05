using System;
using System.Collections.Generic;
using System.Text;
using TrustPay.Domain.Common;

namespace TrustPay.Domain.Events.TransactionsEvents
{
    public record TransactionFailedDomainEvent (Guid TransactionId,
        string ErrorMessage): IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;

    }
}
