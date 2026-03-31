using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Application.Policies.DTOs
{
    public sealed class PolicyResponse
    {
        public Guid Id { get; init; }
        public string PolicyNumber { get; init; } = null!;
        public string Type { get; init; } = null!;
        public string Status { get; init; } = null!;
        public string InsuredFullName { get; init; } = null!;
        public string InsuredDocumentId { get; init; } = null!;
        public int InsuredAge { get; init; }
        public DateOnly CoverageStartDate { get; init; }
        public DateOnly CoverageEndDate { get; init; }
        public decimal MonthlyPremium { get; init; }
        public decimal InsuredAmount { get; init; }
        public decimal AvailableInsuredAmount { get; init; }
        public string? CancellationReason { get; init; }
        public DateOnly? CancellationEffectiveDate { get; init; }
        public Guid? RenewedFromPolicyId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public IEnumerable<PolicyStatusHistoryResponse> StatusHistory { get; init; } = [];
    }

    public sealed class PolicyStatusHistoryResponse
    {
        public string Status { get; init; } = null!;
        public string Notes { get; init; } = null!;
        public DateTime ChangedAt { get; init; }
    }
}
