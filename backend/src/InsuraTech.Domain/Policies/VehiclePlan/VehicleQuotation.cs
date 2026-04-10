namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Resultado transitorio del motor de cotización vehicular (SPEC-010).
/// Contiene la prima base y las primas calculadas para los tres planes.
/// No se persiste; se usa para presentar opciones al agente antes de la selección.
/// </summary>
public sealed class VehicleQuotation
{
    public decimal CommercialValue    { get; }
    public int     VehicleYear        { get; }
    public string  Brand              { get; }
    public int     VehicleAge         { get; }
    /// <summary>"Nuevo" | "Usado Reciente" | "Usado Antiguo"</summary>
    public string  AgeCategory        { get; }
    /// <summary>Tasa técnica aplicada: 0.022 / 0.028 / 0.035.</summary>
    public decimal TechnicalRate      { get; }
    public bool    HasBrandSurcharge  { get; }
    /// <summary>Prima base (Plan Estándar) tras aplicar mínimo y redondeo al mil superior.</summary>
    public decimal BaseMonthlyPremium    { get; }
    /// <summary>Prima mensual Plan Completo = CEILING(base × 1.20 / 1000) × 1000.</summary>
    public decimal CompletePlanPremium   { get; }
    /// <summary>Prima mensual Plan Premium = CEILING(base × 1.45 / 1000) × 1000.</summary>
    public decimal PremiumPlanPremium    { get; }

    public VehicleQuotation(
        decimal commercialValue,
        int     vehicleYear,
        string  brand,
        int     vehicleAge,
        string  ageCategory,
        decimal technicalRate,
        bool    hasBrandSurcharge,
        decimal baseMonthlyPremium,
        decimal completePlanPremium,
        decimal premiumPlanPremium)
    {
        CommercialValue     = commercialValue;
        VehicleYear         = vehicleYear;
        Brand               = brand;
        VehicleAge          = vehicleAge;
        AgeCategory         = ageCategory;
        TechnicalRate       = technicalRate;
        HasBrandSurcharge   = hasBrandSurcharge;
        BaseMonthlyPremium  = baseMonthlyPremium;
        CompletePlanPremium = completePlanPremium;
        PremiumPlanPremium  = premiumPlanPremium;
    }
}
