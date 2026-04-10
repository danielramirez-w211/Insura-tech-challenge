using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.LifePlan;

/// <summary>
/// Snapshot inmutable del plan de vida seleccionado en el momento de contratación.
/// Se embebe dentro de <see cref="Policy"/> y no cambia aunque el catálogo evolucione.
/// </summary>
public sealed class LifePlanSelection : ValueObject
{
    public string PlanId { get; }
    public string PlanName { get; }
    public decimal AnnualPremium { get; }
    public decimal MonthlyPremium { get; }
    public decimal DeathBenefit { get; }
    public decimal FuneralExpenses { get; }
    public decimal BurialExpenses { get; }
    public decimal BeneficiaryCompensation { get; }

    public LifePlanSelection(
        string planId,
        string planName,
        decimal annualPremium,
        decimal monthlyPremium,
        decimal deathBenefit,
        decimal funeralExpenses,
        decimal burialExpenses,
        decimal beneficiaryCompensation)
    {
        PlanId                  = planId;
        PlanName                = planName;
        AnnualPremium           = annualPremium;
        MonthlyPremium          = monthlyPremium;
        DeathBenefit            = deathBenefit;
        FuneralExpenses         = funeralExpenses;
        BurialExpenses          = burialExpenses;
        BeneficiaryCompensation = beneficiaryCompensation;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return PlanId;
        yield return AnnualPremium;
        yield return DeathBenefit;
    }
}
