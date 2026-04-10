namespace InsuraTech.Domain.Policies.LifePlan;

public static class LifePlanCatalog
{
    // AnnualPremium: 120k / 150k / 200k COP → MonthlyPremium: 10k / 12.5k / 16.667k COP
    public static readonly LifePlan PlanVida = new(
        id:                      "plan-vida",
        name:                    "Plan Vida",
        annualPremium:           120_000m,
        deathBenefit:            30_000_000m,
        funeralExpenses:         3_000_000m,
        burialExpenses:          1_500_000m,
        beneficiaryCompensation: 1_500_000m);

    public static readonly LifePlan VidaFamilia = new(
        id:                      "vida-familia",
        name:                    "Vida para la Familia",
        annualPremium:           150_000m,
        deathBenefit:            60_000_000m,
        funeralExpenses:         5_000_000m,
        burialExpenses:          2_500_000m,
        beneficiaryCompensation: 2_500_000m);

    public static readonly LifePlan VidaPremium = new(
        id:                      "vida-premium",
        name:                    "Vida Premium",
        annualPremium:           200_000m,
        deathBenefit:            100_000_000m,
        funeralExpenses:         8_000_000m,
        burialExpenses:          4_000_000m,
        beneficiaryCompensation: 4_000_000m);

    public static readonly IReadOnlyList<LifePlan> All =
        [PlanVida, VidaFamilia, VidaPremium];

    public static LifePlan? FindById(string id) =>
        All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
