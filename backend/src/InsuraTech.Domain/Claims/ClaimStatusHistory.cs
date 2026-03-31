using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Claims
{
    public sealed class ClaimStatusHistory : Entity
    {
        public Guid ClaimId { get; private set; }
        public ClaimStatus Status { get; private set; }
        public string ResponsibleUser { get; private set; } = null!;
        public string? Observations { get; private set; }
        public DateTime ChangedAt { get; private set; }
        private ClaimStatusHistory() { }

        public static ClaimStatusHistory Create(
            Guid claimId, ClaimStatus status, string responsibleUser, string? observation = null)
        { 
            if(string.IsNullOrWhiteSpace(responsibleUser))
                throw new ArgumentNullException("Responsable user is requiered. ", nameof(responsibleUser));
            return new ClaimStatusHistory
            {
                ClaimId = claimId,
                Status = status,
                ResponsibleUser = responsibleUser,
                ChangedAt = DateTime.UtcNow,
            };
        }


    }
}
