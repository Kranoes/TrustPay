using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Application.Common.Models;
using TrustPay.Domain.Common;

namespace TrustPay.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
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
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = Activator.CreateInstance(notificationType, domainEvent);
                if (notification is INotification mediatorNotification)
                {
                    await _publisher.Publish(mediatorNotification,cancellationToken);
                }
            }
            return result;

        }
    }
}
