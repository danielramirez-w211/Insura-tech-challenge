using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Common
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domaninEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomaintEvents => _domaninEvents.AsReadOnly();
        protected void AddDomainEvent(IDomainEvent domaninEvent) =>
            _domaninEvents.Add(domaninEvent);
        public void ClearDomainEvent() => _domaninEvents.Clear();

    }

    public interface IDomainEvent
    {
        Guid Id { get; }
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}
