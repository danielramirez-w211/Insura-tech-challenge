using InsuraTech.Domain.Exceptions;

namespace InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Motor de cotización de pólizas vehiculares (SPEC-010).
/// Aplica tasa técnica por antigüedad, recargo por marca, prima mínima y redondeo al mil superior.
/// </summary>
public static class VehiclePricingService
{
    private const decimal MinPremium       = 800_000m;
    private const decimal BrandSurcharge   = 1.10m;
    private const decimal AnnualDiscount   = 0.95m;
    private const decimal TechRateNew      = 0.022m;   // Nuevo (0 años)
    private const decimal TechRateRecent   = 0.028m;   // Usado Reciente (1-5 años)
    private const decimal TechRateOld      = 0.035m;   // Usado Antiguo (>5 años)

    /// <summary>
    /// Calcula las primas de los tres planes para los datos del vehículo proporcionados.
    /// </summary>
    /// <param name="commercialValue">Valor comercial del vehículo (COP). Debe ser > 0.</param>
    /// <param name="vehicleYear">Año de fabricación del vehículo.</param>
    /// <param name="brand">Marca del vehículo.</param>
    /// <param name="currentYear">Año actual (para calcular antigüedad).</param>
    public static VehicleQuotation Calculate(
        decimal commercialValue,
        int     vehicleYear,
        string  brand,
        int     currentYear)
    {
        if (commercialValue <= 0)
            throw new ArgumentException(
                "El valor comercial del vehículo debe ser mayor a $0.", nameof(commercialValue));

        if (vehicleYear < 1900 || vehicleYear > currentYear)
            throw new ArgumentException(
                $"El año de fabricación debe estar entre 1900 y {currentYear}.", nameof(vehicleYear));

        // PASO 1 — Antigüedad y tasa técnica (RN-01, RN-09)
        int vehicleAge = currentYear - vehicleYear;
        decimal rate;
        string ageCategory;

        if (vehicleAge == 0)        { rate = TechRateNew;    ageCategory = "Nuevo"; }
        else if (vehicleAge <= 5)   { rate = TechRateRecent; ageCategory = "Usado Reciente"; }
        else                        { rate = TechRateOld;    ageCategory = "Usado Antiguo"; }

        // PASO 2 — Prima bruta base (RN-02)
        decimal gross = commercialValue * rate;

        // PASO 3 — Recargo por marca de alta siniestralidad (RN-03)
        bool hasSurcharge = HighSinistrabilityBrands.Contains(brand);
        if (hasSurcharge)
            gross *= BrandSurcharge;

        // PASO 4 — Prima mínima anual (RN-04)
        gross = Math.Max(gross, MinPremium);

        // PASO 5 — Redondeo al mil superior → prima ANUAL base (RN-05, RN-18)
        // La tasa técnica es anual; el resultado es la prima anual, no mensual.
        decimal annualBase     = Math.Ceiling(gross / 1000m) * 1000m;

        // PASO 6 — Primas anuales por plan (RN-06, RN-07)
        decimal annualComplete = Math.Ceiling((annualBase * 1.20m) / 1000m) * 1000m;
        decimal annualPremium_ = Math.Ceiling((annualBase * 1.45m) / 1000m) * 1000m;

        // PASO 6b — Prima mensual = prima anual / 12 (RN-18)
        decimal baseMonthly     = Math.Round(annualBase     / 12m, 0, MidpointRounding.AwayFromZero);
        decimal completeMonthly = Math.Round(annualComplete / 12m, 0, MidpointRounding.AwayFromZero);
        decimal premiumMonthly  = Math.Round(annualPremium_ / 12m, 0, MidpointRounding.AwayFromZero);

        return new VehicleQuotation(
            commercialValue,
            vehicleYear,
            brand,
            vehicleAge,
            ageCategory,
            rate,
            hasSurcharge,
            baseMonthly,
            completeMonthly,
            premiumMonthly,
            annualBase,
            annualComplete,
            annualPremium_);
    }

    /// <summary>
    /// Crea el snapshot de selección para persistir en la póliza.
    /// </summary>
    public static VehiclePlanSelection Select(string planId, VehicleQuotation quotation)
    {
        var plan = VehiclePlanCatalog.FindById(planId)
            ?? throw new InvalidVehiclePlanException(planId);

        decimal finalMonthly = planId.ToLowerInvariant() switch
        {
            "standard" => quotation.BaseMonthlyPremium,
            "complete" => quotation.CompletePlanPremium,
            "premium"  => quotation.PremiumPlanPremium,
            _          => throw new InvalidVehiclePlanException(planId)
        };

        decimal finalAnnual = planId.ToLowerInvariant() switch
        {
            "standard" => quotation.AnnualBase,
            "complete" => quotation.AnnualComplete,
            "premium"  => quotation.AnnualPremium,
            _          => throw new InvalidVehiclePlanException(planId)
        };

        // PASO 7 — Descuento 5% por pago anual (RN-08)
        // El descuento aplica sobre la prima anual: el cliente paga menos si cubre el año en un solo pago.
        decimal annualWithDiscount = finalAnnual * AnnualDiscount;

        return new VehiclePlanSelection(
            planId,
            plan.Name,
            quotation.Brand,
            quotation.VehicleYear,
            quotation.CommercialValue,
            quotation.TechnicalRate,
            quotation.HasBrandSurcharge,
            quotation.BaseMonthlyPremium,
            finalMonthly,
            annualWithDiscount,
            plan.Coverages,
            plan.Assistances);
    }
}
