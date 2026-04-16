using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Application.Claims.DTOs
{
    public sealed class ClaimResponse
    {
        public Guid Id { get; init; }
        public Guid PolicyId { get; init; }
        public string Type { get; init; } = null!;
        public string Status { get; init; } = null!;
        public decimal ClaimedAmount { get; init; }
        public decimal? ApprovedAmount { get; init; }
        public DateOnly IncidentDate { get; init; }
        public string Description { get; init; } = null!;
        public bool HasBeenAppealed { get; init; }
        public string? RejectionReason { get; init; }
        public Guid? CreatedByAdvisorId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public IEnumerable<ClaimStatusHistoryResponse> StatusHistory { get; init; } = [];
    }

    public sealed class ClaimStatusHistoryResponse
    {
        public string Status { get; init; } = null!;
        public string ResponsibleUser { get; init; } = null!;
        public string? Observations { get; init; }
        public DateTime ChangedAt { get; init; }
    }
}
