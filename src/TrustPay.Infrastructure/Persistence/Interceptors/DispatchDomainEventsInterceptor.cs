using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrustPay.Domain.Common;

namespace TrustPay.Infrastructure.Persistence.Interceptors
{
    public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;
        public DispatchDomainEventsInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
        }
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default
            )
        {
            if (eventData.Context is not null)
            {
                await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
            }
            return await base.SavingChangesAsync(eventData,result, cancellationToken);
        }
        private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
        {
            var aggregates = context.ChangeTracker
                .Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();
            var domainEvents = aggregates
                .SelectMany(e => e.DomainEvents)
                .ToList();
            aggregates.ForEach(e => e.ClearDomainEvents());
            foreach(var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent,cancellationToken);
            }

        }

    }
}
