using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Events
{
    public sealed class PolicyExpiringSoonEvent : IDomainEvent
    {


        public Guid EventId { get; } = Guid.NewGuid();
        public Guid Id => EventId;
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(PolicyExpiringSoonEvent);

        public Guid PolicyId { get; }
        public string PolicyNumber { get; }
        public string InsuredFullName { get; }
        public DateOnly ExpirationDate { get; }
        public int DaysRemaining { get; }

        public PolicyExpiringSoonEvent(Guid policyId, string policyNumber, string insuredFullName, DateOnly expirationDate, int daysRemaining)
        {
            PolicyId = policyId;
            PolicyNumber = policyNumber;
            InsuredFullName = insuredFullName;
            ExpirationDate = expirationDate;
            DaysRemaining = daysRemaining;
        }



    }
}
