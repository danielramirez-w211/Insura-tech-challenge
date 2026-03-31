using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Policies;

namespace InsuraTech.Domain.Common
{
    public sealed class PolicyStatusHistory : Entity
    {
        public Guid PolicyId { get; private set; }
        public PolicyStatus Status { get; private set; }
        public string Notes { get; private set; } = null!;
        public DateTime ChangedAt { get; private set; }

        private PolicyStatusHistory() { }

        public static PolicyStatusHistory Create(Guid policyId, PolicyStatus status, string notes)
        {
            return new PolicyStatusHistory
            {
                PolicyId = policyId,
                Status = status,
                Notes = notes,
                ChangedAt = DateTime.UtcNow
            };
        }
    }
}
