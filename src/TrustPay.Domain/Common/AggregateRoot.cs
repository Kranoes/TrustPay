using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Common
{
    
    public interface IDomainEvent 
    {
        Guid EventId { get; }
        DateTime OccurredOnUtc { get; }
    }
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
       
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
    public abstract class AggregateRoot<TId> : AggregateRoot
    where TId : notnull
    {
        public TId Id { get; protected set; }

        protected AggregateRoot(TId id) => Id = id;
        protected AggregateRoot() { } 
    }
}
