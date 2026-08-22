using TrustPay.Domain.Common;
namespace TrustPay.Domain.Events.OrderEvents;

public record OrderCompletedDomainEvent(Guid OrderId, Guid CustomerId, Guid ExecutorId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}