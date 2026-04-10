using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.VehiclePlan;

public sealed class VehiclePlan : ValueObject
{
    public string Id { get; }
    public string Name { get; }
    /// <summary>Multiplicador sobre la prima base: 1.00 / 1.20 / 1.45.</summary>
    public decimal PriceMultiplier { get; }
    /// <summary>Coberturas incluidas en el plan (acumulativas — cada plan incluye las del anterior).</summary>
    public IReadOnlyList<string> Coverages { get; }
    /// <summary>Servicios de asistencia adicionales (vacío en Plan Estándar).</summary>
    public IReadOnlyList<string> Assistances { get; }

    internal VehiclePlan(
        string id,
        string name,
        decimal priceMultiplier,
        IReadOnlyList<string> coverages,
        IReadOnlyList<string> assistances)
    {
        Id              = id;
        Name            = name;
        PriceMultiplier = priceMultiplier;
        Coverages       = coverages;
        Assistances     = assistances;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return Id;
    }

    public override string ToString() => $"{Name} (×{PriceMultiplier:F2})";
}
