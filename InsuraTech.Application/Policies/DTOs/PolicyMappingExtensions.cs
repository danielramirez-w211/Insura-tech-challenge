using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Policies;

namespace InsuraTech.Application.Policies.DTOs
{
    public static class PolicyMappingExtensions
    {
        public static PolicyResponse ToResponse(this Policy policy) =>
                new()
                {
                    Id = policy.Id,
                    PolicyNumber = policy.Number.Value,
                    Type = policy.Type.ToString(),
                    Status = policy.Status.ToString(),
                    InsuredFullName = policy.Insured.FullName,
                    InsuredDocumentId = policy.Insured.DocumentId,
                    InsuredAge = policy.Insured.Age,
                    CoverageStartDate = policy.Coverage.StartDate,
                    CoverageEndDate = policy.Coverage.EndDate,
                    MonthlyPremium = policy.MonthlyPremium,
                    InsuredAmount = policy.InsuredAmount,
                    AvailableInsuredAmount = policy.AvailableInsuredAmount,
                    CancellationReason = policy.CancellationReason,
                    CancellationEffectiveDate = policy.CancellationEffectiveDate,
                    RenewedFromPolicyId = policy.RenewedFromPolicyId,
                    CreatedAt = policy.CreatedAt,
                    UpdatedAt = policy.UpdatedAt,
                    StatusHistory = policy.StatusHistory.Select(h => new PolicyStatusHistoryResponse
                    {
                        Status = h.Status.ToString(),
                        Notes = h.Notes,
                        ChangedAt = h.ChangedAt
                    })
                };
    }
}
