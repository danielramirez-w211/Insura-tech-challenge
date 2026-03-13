using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Events
{
    public sealed class ClaimRegisteredEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(ClaimRegisteredEvent);

        public Guid ClaimId { get; }
        public Guid PolicyId {  get; }
        public string PolicyNumer {  get; }
        public Decimal ClaimAmount {  get; }

        public ClaimRegisteredEvent(Guid id, DateTime occurredOn, Guid claimId, Guid policyId, string policyNumer, decimal claimAmount)
        {
            Id = id;
            OccurredOn = occurredOn;
            ClaimId = claimId;
            PolicyId = policyId;
            PolicyNumer = policyNumer;
            ClaimAmount = claimAmount;
        }
    }
}
