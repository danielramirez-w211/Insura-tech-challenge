using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.HealthPlan;

public sealed class HealthPlanSelection : ValueObject
{
    public string PlanId { get; }
    public string PlanName { get; }
    public decimal BaseAmount { get; }
    public int AgeFactorPercentage { get; }
    public decimal AgeFactorAmount { get; }
    public decimal FinalAmount { get; }

    internal HealthPlanSelection(
        string planId,
        string planName,
        decimal baseAmount,
        int ageFactorPercentage,
        decimal ageFactorAmount,
        decimal finalAmount)
    {
        PlanId = planId;
        PlanName = planName;
        BaseAmount = baseAmount;
        AgeFactorPercentage = ageFactorPercentage;
        AgeFactorAmount = ageFactorAmount;
        FinalAmount = finalAmount;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return PlanId;
        yield return BaseAmount;
        yield return AgeFactorPercentage;
        yield return FinalAmount;
    }
}
