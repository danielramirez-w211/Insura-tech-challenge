using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Common
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomaninEvent> _domaninEvents = new();
        public IReadOnlyCollection<IDomaninEvent> DomaintEvents => _domaninEvents.AsReadOnly();
        protected void AddDomainEvent(IDomaninEvent domaninEvent) =>
            _domaninEvents.Add(domaninEvent);
        public void ClearDomainEvent() => _domaninEvents.Clear();

    }

    public interface IDomaninEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}
