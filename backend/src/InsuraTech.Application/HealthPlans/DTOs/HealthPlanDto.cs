namespace InsuraTech.Application.HealthPlans.DTOs;

public sealed class HealthPlanDto
{
    public string PlanId { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public decimal BaseAmount { get; init; }
}

public sealed class HealthPlanCalculationDto
{
    public string PlanId { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public decimal BaseAmount { get; init; }
    public int AgeFactorPercentage { get; init; }
    public decimal AgeFactorAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public int InsuredAge { get; init; }
    /// <summary>Prima mensual = FinalAmount / 12 (periodo fijo). Ver QuotationService.</summary>
    public decimal MonthlyPremium { get; init; }
    /// <summary>Duración fija de cobertura en días (siempre 365).</summary>
    public int DurationDays { get; init; }
}
