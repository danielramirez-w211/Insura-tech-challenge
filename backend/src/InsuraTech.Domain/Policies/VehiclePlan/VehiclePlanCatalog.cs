namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Catálogo estático de los tres planes de cobertura vehicular (SPEC-010).
/// Coberturas acumulativas: cada plan incluye todo lo del plan anterior.
/// </summary>
public static class VehiclePlanCatalog
{
    private static readonly IReadOnlyList<string> BaseCoverages =
    [
        "Robo",
        "Pérdida parcial",
        "Pérdida total",
        "Daños a terceros",
        "Rayones a carrocería"
    ];

    private static readonly IReadOnlyList<string> CompleteCoverages =
    [
        "Robo",
        "Pérdida parcial",
        "Pérdida total",
        "Daños a terceros",
        "Rayones a carrocería",
        "Pérdida de llaves",
        "Daño mecánico",
        "Daño eléctrico"
    ];

    public static readonly VehiclePlan Standard = new(
        id:             "standard",
        name:           "Plan Estándar",
        priceMultiplier: 1.00m,
        coverages:      BaseCoverages,
        assistances:    []);

    public static readonly VehiclePlan Complete = new(
        id:             "complete",
        name:           "Plan Completo",
        priceMultiplier: 1.20m,
        coverages:      CompleteCoverages,
        assistances:    ["Grúa"]);

    public static readonly VehiclePlan Premium = new(
        id:             "premium",
        name:           "Plan Premium",
        priceMultiplier: 1.45m,
        coverages:      CompleteCoverages,
        assistances:    ["Grúa", "Carro de repuesto", "Mecánico a casa", "Conductor elegido"]);

    public static readonly IReadOnlyList<VehiclePlan> All = [Standard, Complete, Premium];

    public static VehiclePlan? FindById(string id) =>
        All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
