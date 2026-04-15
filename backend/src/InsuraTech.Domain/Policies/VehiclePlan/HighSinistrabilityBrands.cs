namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Lista configurable de marcas de alta siniestralidad (RN-10, SPEC-010).
/// El recargo aplicado es del 10% sobre la prima bruta.
/// <para>
/// NOTA: Esta lista es ilustrativa y debe ser revisada y actualizada por el equipo actuarial.
/// En una iteración futura migrar a configuración externa (base de datos o archivo de configuración).
/// </para>
/// </summary>
public static class HighSinistrabilityBrands
{
    private static readonly HashSet<string> _brands = new(StringComparer.OrdinalIgnoreCase)
    {
        "Renault",
        "Chevrolet"
    };

    public static bool Contains(string brand) =>
        !string.IsNullOrWhiteSpace(brand) && _brands.Contains(brand.Trim());
}
