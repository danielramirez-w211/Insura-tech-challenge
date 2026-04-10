using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Snapshot inmutable del plan vehicular seleccionado en el momento de contratación.
/// Se embebe dentro de <see cref="Policy"/> y no cambia aunque el catálogo o el motor evolucionen.
/// </summary>
public sealed class VehiclePlanSelection : ValueObject
{
    public string  PlanId                    { get; }
    public string  PlanName                  { get; }
    public string  VehicleBrand              { get; }
    public int     VehicleYear               { get; }
    public decimal CommercialValue           { get; }
    public decimal TechnicalRate             { get; }
    public bool    HasBrandSurcharge         { get; }
    /// <summary>Prima base Plan Estándar (referencia).</summary>
    public decimal BaseMonthlyPremium        { get; }
    /// <summary>Prima mensual del plan seleccionado.</summary>
    public decimal FinalMonthlyPremium       { get; }
    /// <summary>Prima mensual con descuento del 5% por pago anual (RN-08).</summary>
    public decimal AnnualPremiumWithDiscount  { get; }
    /// <summary>Coberturas incluidas en el plan (snapshot en tiempo de contratación).</summary>
    public IReadOnlyList<string> Coverages   { get; }
    /// <summary>Servicios de asistencia del plan contratado (vacío para Estándar).</summary>
    public IReadOnlyList<string> Assistances { get; }

    public VehiclePlanSelection(
        string  planId,
        string  planName,
        string  vehicleBrand,
        int     vehicleYear,
        decimal commercialValue,
        decimal technicalRate,
        bool    hasBrandSurcharge,
        decimal baseMonthlyPremium,
        decimal finalMonthlyPremium,
        decimal annualPremiumWithDiscount,
        IReadOnlyList<string> coverages,
        IReadOnlyList<string> assistances)
    {
        PlanId                   = planId;
        PlanName                 = planName;
        VehicleBrand             = vehicleBrand;
        VehicleYear              = vehicleYear;
        CommercialValue          = commercialValue;
        TechnicalRate            = technicalRate;
        HasBrandSurcharge        = hasBrandSurcharge;
        BaseMonthlyPremium       = baseMonthlyPremium;
        FinalMonthlyPremium      = finalMonthlyPremium;
        AnnualPremiumWithDiscount = annualPremiumWithDiscount;
        Coverages                = coverages;
        Assistances              = assistances;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return PlanId;
        yield return CommercialValue;
        yield return FinalMonthlyPremium;
    }
}
