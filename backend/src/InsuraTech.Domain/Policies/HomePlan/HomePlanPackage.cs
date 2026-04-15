namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Paquete de cobertura predefinido para pólizas de hogar (SPEC-012).
/// </summary>
public sealed record HomePlanPackage(
    string Id,
    string Name,
    IReadOnlyList<HomeCoverage> Coverages);
