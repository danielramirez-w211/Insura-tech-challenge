namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Resultado transitorio del motor de cotización vehicular (SPEC-010).
/// Contiene la prima mensual y la prima anual para los tres planes (RN-18).
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
    /// <summary>Tasa técnica aplicada: 0.022 / 0.028 / 0.035 (tasa ANUAL).</summary>
    public decimal TechnicalRate      { get; }
    public bool    HasBrandSurcharge  { get; }

    // ── Primas mensuales (prima anual / 12) ────────────────────────────────
    /// <summary>Prima mensual Plan Estándar = primaAnualBase / 12.</summary>
    public decimal BaseMonthlyPremium    { get; }
    /// <summary>Prima mensual Plan Completo = primaAnualCompleto / 12.</summary>
    public decimal CompletePlanPremium   { get; }
    /// <summary>Prima mensual Plan Premium = primaAnualPremium / 12.</summary>
    public decimal PremiumPlanPremium    { get; }

    // ── Primas anuales (CEILING al mil superior) ────────────────────────────
    /// <summary>Prima anual base Plan Estándar = CEILING(gross / 1000) × 1000.</summary>
    public decimal AnnualBase     { get; }
    /// <summary>Prima anual Plan Completo = CEILING(base × 1.20 / 1000) × 1000.</summary>
    public decimal AnnualComplete { get; }
    /// <summary>Prima anual Plan Premium = CEILING(base × 1.45 / 1000) × 1000.</summary>
    public decimal AnnualPremium  { get; }

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
        decimal premiumPlanPremium,
        decimal annualBase,
        decimal annualComplete,
        decimal annualPremium)
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
        AnnualBase          = annualBase;
        AnnualComplete      = annualComplete;
        AnnualPremium       = annualPremium;
    }
}
