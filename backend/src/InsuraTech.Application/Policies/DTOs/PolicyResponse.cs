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
        public string InsuredFirstName { get; init; } = null!;
        public string InsuredLastName { get; init; } = null!;
        public string InsuredDocumentType {get; init;} = null!;

        public string InsuredDocumentId { get; init; } = null!;
        public int InsuredAge { get; init; }
        public string InsuredGender { get; init; } = null!;
        public string InsuredAddress { get; init; } = null!;
        public string InsuredCityName { get; init; } = null!;
        public string InsuredPostalCode { get; init; } = null!;
        public string InsuredDepartment { get; init; } = null!;
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
        public HealthPlanSelectionDto? HealthPlan { get; init; }
    public TravelPlanSelectionDto? TravelPlan { get; init; }
    public VehiclePlanSelectionDto? VehiclePlan { get; init; }
    }

    public sealed class PolicyStatusHistoryResponse
    {
        public string Status { get; init; } = null!;
        public string Notes { get; init; } = null!;
        public DateTime ChangedAt { get; init; }
    }

    public sealed class TravelPlanSelectionDto
    {
        public string TripType           { get; init; } = null!;
        public string? Continent         { get; init; }
        public int DurationDays          { get; init; }
        public decimal? BasePriceUsd     { get; init; }
        public decimal BasePriceCop      { get; init; }
        public decimal DailyIncrementCop { get; init; }
        public decimal TotalPriceCop     { get; init; }
        public decimal? TrmUsed          { get; init; }
        public DateOnly? TrmDate         { get; init; }
        public DateTime CalculatedAt     { get; init; }
    }

    public sealed class VehiclePlanSelectionDto
    {
        public string  PlanId                    { get; init; } = null!;
        public string  PlanName                  { get; init; } = null!;
        public string  VehicleBrand              { get; init; } = null!;
        public int     VehicleYear               { get; init; }
        public decimal CommercialValue           { get; init; }
        public decimal TechnicalRate             { get; init; }
        public bool    HasBrandSurcharge         { get; init; }
        public decimal BaseMonthlyPremium        { get; init; }
        public decimal FinalMonthlyPremium       { get; init; }
        public decimal AnnualPremiumWithDiscount  { get; init; }
        public IEnumerable<string> Coverages     { get; init; } = [];
        public IEnumerable<string> Assistances   { get; init; } = [];
    }

    public sealed class HealthPlanSelectionDto
    {
        public string PlanId { get; init; } = null!;
        public string PlanName { get; init; } = null!;
        public decimal BaseAmount { get; init; }
        public int AgeFactorPercentage { get; init; }
        public decimal AgeFactorAmount { get; init; }
        public decimal FinalAmount { get; init; }
    }
}
