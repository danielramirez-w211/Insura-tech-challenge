using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Claims
{
    public sealed class Claim : AggregateRoot
    {
        public const int MaxOpenClaimsPerPolicy = 3;

        public Guid PolicyId { get; private set; }
        public ClaimType Type { get; private set; }
        public ClaimStatus Status { get; private set; }
        public decimal ClaimedAmount { get; private set; }
        public decimal? ApprovedAmount { get; private set; }
        public DateOnly IncidentDate { get; private set; }
        public string Description { get; private set; } = null!;
        public bool HasBeenAppealed { get; private set; }
        public string? RejectionReason { get; private set; }

        private readonly List<ClaimStatusHistory> _statusHistory = new();



    }
}
