namespace InsuraTech.Domain.Policies.HealthPlan;

public static class HealthPlanCatalog
{
    public static readonly HealthPlan Basic          = new("basic",            "Básico",           300_000m);
    public static readonly HealthPlan SaludGlobal    = new("salud-global",     "Salud Global",     380_000m);
    public static readonly HealthPlan SaludPremium   = new("salud-premium",    "Salud Premium",    450_000m);
    public static readonly HealthPlan SaludVidaTotal = new("salud-vida-total", "Salud Vida Total", 600_000m);

    public static readonly IReadOnlyList<HealthPlan> All =
        [Basic, SaludGlobal, SaludPremium, SaludVidaTotal];

    public static HealthPlan? FindById(string id) =>
        All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
