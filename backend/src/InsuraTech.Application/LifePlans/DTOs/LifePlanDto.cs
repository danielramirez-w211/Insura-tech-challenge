namespace InsuraTech.Application.LifePlans.DTOs;

public sealed class LifePlanDto
{
    public string PlanId { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public decimal AnnualPremium { get; init; }
    public decimal MonthlyPremium { get; init; }
    public decimal DeathBenefit { get; init; }
    public decimal FuneralExpenses { get; init; }
    public decimal BurialExpenses { get; init; }
    public decimal BeneficiaryCompensation { get; init; }
}

public sealed class LifePlanCalculationDto
{
    public string PlanId { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public int InsuredAge { get; init; }
    public decimal AnnualPremium { get; init; }
    /// <summary>Prima mensual = AnnualPremium / 12. Ver QuotationService / LifePlan.MonthlyPremium.</summary>
    public decimal MonthlyPremium { get; init; }
    /// <summary>Duración fija de cobertura en días (siempre 365).</summary>
    public int DurationDays { get; init; }
    public decimal DeathBenefit { get; init; }
    public decimal FuneralExpenses { get; init; }
    public decimal BurialExpenses { get; init; }
    public decimal BeneficiaryCompensation { get; init; }
}
