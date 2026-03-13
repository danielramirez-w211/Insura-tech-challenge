using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Events
{
    public sealed class PolicyActivatedEvent : IDomainEvent
    {


        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(PolicyActivatedEvent);

        public Guid PolicyId           {  get; }
        public string PolicyNumber     {  get; }
        public string InsuredFullNamed {  get; }
        public string InsuredDocumentId { get; }

        public PolicyActivatedEvent(Guid policyId, string policyNumber, string insuredFullNamed, string insuredDocumentId)
        {
            PolicyId = policyId;
            PolicyNumber = policyNumber;
            InsuredFullNamed = insuredFullNamed;
            InsuredDocumentId = insuredDocumentId;
        }

    }
}
