namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Catálogo estático de los tres paquetes de cobertura de hogar (SPEC-012).
/// Análogo a VehiclePlanCatalog de SPEC-010.
/// </summary>
public static class HomePlanPackageCatalog
{
    public static readonly HomePlanPackage Basic = new(
        Id:        "basic",
        Name:      "Paquete Básico",
        Coverages: [HomeCoverage.FireExplosion, HomeCoverage.Plumbing]);

    public static readonly HomePlanPackage Standard = new(
        Id:        "standard",
        Name:      "Paquete Estándar",
        Coverages: [HomeCoverage.FireExplosion, HomeCoverage.Plumbing,
                    HomeCoverage.Theft, HomeCoverage.ElectricalDamage]);

    public static readonly HomePlanPackage Premium = new(
        Id:        "premium",
        Name:      "Paquete Premium",
        Coverages: [HomeCoverage.FireExplosion, HomeCoverage.Plumbing,
                    HomeCoverage.Theft, HomeCoverage.ElectricalDamage,
                    HomeCoverage.CivilLiability, HomeCoverage.GlassBreakage,
                    HomeCoverage.HomeAssistance, HomeCoverage.Uninhabitability,
                    HomeCoverage.LegalDefense]);

    public static readonly IReadOnlyList<HomePlanPackage> All = [Basic, Standard, Premium];

    public static HomePlanPackage? FindById(string id) =>
        All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
