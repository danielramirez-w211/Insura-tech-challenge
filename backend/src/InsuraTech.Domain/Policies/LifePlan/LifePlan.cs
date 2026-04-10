using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.LifePlan;

public sealed class LifePlan : ValueObject
{
    public string Id { get; }
    public string Name { get; }
    /// <summary>Prima anual fija (COP) — fuente de verdad del catálogo.</summary>
    public decimal AnnualPremium { get; }
    /// <summary>Prima mensual = AnnualPremium / 12.</summary>
    public decimal MonthlyPremium { get; }
    /// <summary>Suma asegurada por muerte (COP).</summary>
    public decimal DeathBenefit { get; }
    public decimal FuneralExpenses { get; }
    public decimal BurialExpenses { get; }
    public decimal BeneficiaryCompensation { get; }

    internal LifePlan(
        string id,
        string name,
        decimal annualPremium,
        decimal deathBenefit,
        decimal funeralExpenses,
        decimal burialExpenses,
        decimal beneficiaryCompensation)
    {
        Id                      = id;
        Name                    = name;
        AnnualPremium           = annualPremium;
        MonthlyPremium          = Math.Round(annualPremium / 12m, 0, MidpointRounding.AwayFromZero);
        DeathBenefit            = deathBenefit;
        FuneralExpenses         = funeralExpenses;
        BurialExpenses          = burialExpenses;
        BeneficiaryCompensation = beneficiaryCompensation;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return Id;
    }

    public override string ToString() => $"{Name} (${MonthlyPremium:N0}/mes)";
}
