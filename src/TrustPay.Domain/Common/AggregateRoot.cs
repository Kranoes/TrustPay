using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Domain.Common
{
    /// <summary>
    /// Маркерный интерфейс для всех доменных событий системы. Доменные события представляют собой значимые события, которые происходят в доменной модели и могут быть использованы для уведомления других частей системы о произошедших изменениях.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOnUtc { get; }
    }
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        /// <summary>
        /// Коллекция событий, зарегистрированных агрегатом во время выполнения бизнес-методов.
        /// Инкапсулирована через IReadOnlyCollection.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        /// <summary>
        /// Добавление нового события (вызывается только внутри бизнес-методов агрегата).
        /// </summary>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        /// <summary>
        /// Очистка списка событий после их публикации в DbContext / Unit of Work.
        /// </summary>
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
