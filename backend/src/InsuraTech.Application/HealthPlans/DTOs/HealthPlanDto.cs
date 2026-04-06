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
}
