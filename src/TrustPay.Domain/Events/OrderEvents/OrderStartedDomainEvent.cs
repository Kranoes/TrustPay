using TrustPay.Domain.Common;
namespace TrustPay.Domain.Events.OrderEvents;

public record OrderStartedDomainEvent(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}