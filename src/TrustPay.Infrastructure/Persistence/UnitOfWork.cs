namespace TrustPay.Infrastructure.Persistence;

using System.Collections.Concurrent;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Common;

public class UnitOfWork : IUnitOfWork
{
    private static readonly ConcurrentDictionary<Type, Type> NotificationTypeCache = new();
    private readonly TrustPayDbContext _context;
    private readonly IPublisher _publisher;

    public UnitOfWork(TrustPayDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.ClearDomainEvents());

        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var notificationType = NotificationTypeCache.GetOrAdd(
                eventType,
                type => typeof(DomainEventNotification<>).MakeGenericType(type));

            var notification = Activator.CreateInstance(notificationType, domainEvent)
                ?? throw new InvalidOperationException($"Не удалось создать обертку для события {eventType.Name}.");

            if (notification is not INotification mediatorNotification)
            {
                throw new InvalidOperationException($"Созданный тип {notificationType.Name} не реализует INotification.");
            }

            await _publisher.Publish(mediatorNotification, cancellationToken);
        }

        return result;
    }
}