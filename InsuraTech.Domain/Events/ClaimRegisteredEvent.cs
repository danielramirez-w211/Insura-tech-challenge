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
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid Id => EventId;

        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(ClaimRegisteredEvent);

        public Guid ClaimId { get; }
        public Guid PolicyId {  get; }
        public string PolicyNumer {  get; }
        public Decimal ClaimAmount {  get; }

        public ClaimRegisteredEvent(Guid claimId, Guid policyId, string policyNumer, decimal claimAmount)
        {
            ClaimId = claimId;
            PolicyId = policyId;
            PolicyNumer = policyNumer;
            ClaimAmount = claimAmount;
        }
    }
}
