using MediatR;

namespace InsuraTech.Domain.Common
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected void AddDomainEvent(IDomainEvent domainEvent) =>
            _domainEvents.Add(domainEvent);
        public void ClearDomainEvent() => _domainEvents.Clear();
    }

    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}
