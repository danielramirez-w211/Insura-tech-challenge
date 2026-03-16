using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Events
{
    public sealed class PolicyCancelledEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid Id => EventId;

        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(PolicyCancelledEvent);

        public Guid PolicyId { get; }
        public string PolicyNumber { get; }
        public string Reason { get; }
        public DateOnly EffectiveDate { get; }

       public PolicyCancelledEvent(Guid policyId, string policyNumber,
        string reason, DateOnly effectiveDate)
    {
        PolicyId = policyId;
        PolicyNumber = policyNumber;
        Reason = reason;
        EffectiveDate = effectiveDate;
    }
    }
}
