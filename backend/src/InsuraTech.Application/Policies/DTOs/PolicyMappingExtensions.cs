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
                    PolicyNumber                = policy.Number.Value,
                    Type                        = policy.Type.ToString(),
                    Status                      = policy.Status.ToString(),
                    InsuredFirstName            = policy.Insured.FirstName,
                    InsuredLastName             = policy.Insured.LastName,
                    InsuredDocumentType         = policy.Insured.DocumentType,
                    InsuredDocumentId           = policy.Insured.DocumentId,
                    InsuredAge                  = policy.Insured.Age,
                    CoverageStartDate           = policy.Coverage.StartDate,
                    CoverageEndDate             = policy.Coverage.EndDate,
                    MonthlyPremium              = policy.MonthlyPremium,
                    InsuredAmount               = policy.InsuredAmount,
                    AvailableInsuredAmount      = policy.AvailableInsuredAmount,
                    CancellationReason          = policy.CancellationReason,
                    CancellationEffectiveDate   = policy.CancellationEffectiveDate,
                    RenewedFromPolicyId         = policy.RenewedFromPolicyId,
                    CreatedAt                   = policy.CreatedAt,
                    UpdatedAt                   = policy.UpdatedAt,
                    StatusHistory               = policy.StatusHistory.Select(h => new PolicyStatusHistoryResponse
                    {
                        Status      = h.Status.ToString(),
                        Notes       = h.Notes,
                        ChangedAt   = h.ChangedAt
                    }),
                    HealthPlan = policy.HealthPlan is null ? null : new HealthPlanSelectionDto
                    {
                        PlanId              = policy.HealthPlan.PlanId,
                        PlanName            = policy.HealthPlan.PlanName,
                        BaseAmount          = policy.HealthPlan.BaseAmount,
                        AgeFactorPercentage = policy.HealthPlan.AgeFactorPercentage,
                        AgeFactorAmount     = policy.HealthPlan.AgeFactorAmount,
                        FinalAmount         = policy.HealthPlan.FinalAmount
                    },
                    TravelPlan = policy.TravelPlan is null ? null : new TravelPlanSelectionDto
                    {
                        TripType          = policy.TravelPlan.TripType.ToString(),
                        Continent         = policy.TravelPlan.Continent?.ToString(),
                        DurationDays      = policy.TravelPlan.DurationDays,
                        BasePriceUsd      = policy.TravelPlan.BasePriceUsd,
                        BasePriceCop      = policy.TravelPlan.BasePriceCop,
                        DailyIncrementCop = policy.TravelPlan.DailyIncrementCop,
                        TotalPriceCop     = policy.TravelPlan.TotalPriceCop,
                        TrmUsed           = policy.TravelPlan.TrmUsed,
                        TrmDate           = policy.TravelPlan.TrmDate,
                        CalculatedAt      = policy.TravelPlan.CalculatedAt
                    }
                };
    }
}
