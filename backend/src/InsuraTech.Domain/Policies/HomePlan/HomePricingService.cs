namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Motor de cotización de pólizas de hogar (SPEC-012).
/// Prima base = 1% del valor del predio. Coberturas variables y fijas se suman a la prima base.
/// Multiplicadores de riesgo: antigüedad, estrato, habitantes, multicobertura.
/// </summary>
public static class HomePricingService
{
    // ── Tasas variables (sobre prima base) ────────────────────────────────────
    private const decimal TheftRate          = 0.12m;
    private const decimal PlumbingRate       = 0.05m;
    private const decimal AestheticRate      = 0.03m;
    private const decimal ElectricalRate     = 0.07m;
    private const decimal UninhabitableRate  = 0.04m;

    // ── Costos fijos (COP) ────────────────────────────────────────────────────
    private const decimal GlassBreakageFixed    = 50_000m;
    private const decimal LegalDefenseFixed     = 30_000m;
    private const decimal HomeAssistanceFixed   = 45_000m;
    private const decimal WaterDamageFixed      = 65_000m;
    private const decimal CivilLiabilityLow     = 100_000m;  // Estrato 1-2
    private const decimal CivilLiabilityMid     = 175_000m;  // Estrato 3-4
    private const decimal CivilLiabilityHigh    = 250_000m;  // Estrato 5-6

    // ── Multiplicadores ───────────────────────────────────────────────────────
    private const decimal NewBuildingDiscount    = 0.95m;   // Antigüedad < 10 años: -5%
    private const decimal OldBuildingPlumbingSurcharge    = 1.10m;  // Antigüedad > 30 años: +10% en Plomería y Eléctrico
    private const decimal HighStratumTheftSurcharge       = 1.10m;  // Estrato 5-6: +10% en Robo
    private const decimal HighOccupancyCharge   = 30_000m;  // > 5 habitantes: $30.000 fijo
    private const decimal MultiCoverageDiscount = 0.95m;   // > 5 coberturas: -5%
    private const int     MultiCoverageThreshold = 5;

    /// <summary>
    /// Calcula la prima mensual para una póliza de hogar con las coberturas seleccionadas.
    /// </summary>
    public static HomeQuotation Calculate(
        decimal                     propertyValue,
        int                         constructionYear,
        int                         stratum,
        int                         occupants,
        HomePropertyType            propertyType,
        IReadOnlyList<HomeCoverage> selectedCoverages,
        int                         currentYear)
    {
        // ── Validaciones ──────────────────────────────────────────────────────
        if (propertyValue <= 0)
            throw new ArgumentException(
                "El valor comercial del predio debe ser mayor a $0.", nameof(propertyValue));
        if (constructionYear < 1900 || constructionYear > currentYear)
            throw new ArgumentException(
                $"El año de construcción debe estar entre 1900 y {currentYear}.", nameof(constructionYear));
        if (stratum < 1 || stratum > 6)
            throw new ArgumentException(
                "El estrato debe ser un valor entre 1 y 6.", nameof(stratum));
        if (occupants < 1)
            throw new ArgumentException(
                "El número de habitantes debe ser al menos 1.", nameof(occupants));
        if (selectedCoverages is null || selectedCoverages.Count == 0)
            throw new ArgumentException(
                "Debe seleccionar al menos una cobertura.", nameof(selectedCoverages));

        var multipliers = new List<string>();

        // ── PASO 1 — Prima base (1% del valor del predio) ────────────────────
        int     propertyAge        = currentYear - constructionYear;
        decimal basePremium        = propertyValue * 0.01m;

        // ── PASO 2 — Coberturas variables y fijas ────────────────────────────
        decimal theftCost       = 0m;
        decimal plumbingCost    = 0m;
        decimal electricalCost  = 0m;
        decimal coverageExtra   = 0m;

        foreach (var coverage in selectedCoverages)
        {
            switch (coverage)
            {
                case HomeCoverage.FireExplosion:
                    // Incluida en la prima base — sin cargo adicional
                    break;
                case HomeCoverage.Theft:
                    theftCost = basePremium * TheftRate;
                    break;
                case HomeCoverage.Plumbing:
                    plumbingCost = basePremium * PlumbingRate;
                    break;
                case HomeCoverage.AestheticDamage:
                    coverageExtra += basePremium * AestheticRate;
                    break;
                case HomeCoverage.ElectricalDamage:
                    electricalCost = basePremium * ElectricalRate;
                    break;
                case HomeCoverage.GlassBreakage:
                    coverageExtra += GlassBreakageFixed;
                    break;
                case HomeCoverage.CivilLiability:
                    coverageExtra += stratum <= 2 ? CivilLiabilityLow
                                   : stratum <= 4 ? CivilLiabilityMid
                                                  : CivilLiabilityHigh;
                    break;
                case HomeCoverage.Uninhabitability:
                    coverageExtra += basePremium * UninhabitableRate;
                    break;
                case HomeCoverage.LegalDefense:
                    coverageExtra += LegalDefenseFixed;
                    break;
                case HomeCoverage.HomeAssistance:
                    coverageExtra += HomeAssistanceFixed;
                    break;
                case HomeCoverage.WaterDamageExpert:
                    coverageExtra += WaterDamageFixed;
                    break;
            }
        }

        // ── PASO 3 — Multiplicador antigüedad sobre Plomería y Eléctrico ─────
        if (propertyAge > 30 && (plumbingCost > 0 || electricalCost > 0))
        {
            plumbingCost   *= OldBuildingPlumbingSurcharge;
            electricalCost *= OldBuildingPlumbingSurcharge;
            multipliers.Add("Recargo 10% antigüedad (> 30 años) en Fontanería/Eléctrico");
        }

        // ── PASO 4 — Multiplicador estrato sobre Robo ─────────────────────────
        if (stratum >= 5 && theftCost > 0)
        {
            theftCost *= HighStratumTheftSurcharge;
            multipliers.Add("Recargo 10% por estrato alto (5-6) en Robo y Hurto");
        }

        decimal total = basePremium + theftCost + plumbingCost + electricalCost + coverageExtra;

        // ── PASO 3b — Descuento por inmueble nuevo (< 10 años) sobre el total ─
        if (propertyAge < 10)
        {
            total *= NewBuildingDiscount;
            multipliers.Add("Descuento 5% inmueble nuevo (< 10 años)");
        }

        // ── PASO 5 — Cargo por alta ocupación (> 5 habitantes) ───────────────
        if (occupants > 5)
        {
            total += HighOccupancyCharge;
            multipliers.Add($"Cargo fijo $30.000 por alta ocupación ({occupants} habitantes)");
        }

        // ── PASO 6 — Descuento multicobertura (> 5 coberturas) ───────────────
        if (selectedCoverages.Count > MultiCoverageThreshold)
        {
            total *= MultiCoverageDiscount;
            multipliers.Add($"Descuento 5% multicobertura ({selectedCoverages.Count} coberturas seleccionadas)");
        }

        // ── PASO 7 — Redondeo al mil superior ────────────────────────────────
        decimal finalPremium = Math.Ceiling(total / 1000m) * 1000m;

        return new HomeQuotation(
            propertyValue:       propertyValue,
            constructionYear:    constructionYear,
            propertyAge:         propertyAge,
            stratum:             stratum,
            occupants:           occupants,
            propertyType:        propertyType,
            baseMonthlyPremium:  Math.Ceiling(basePremium / 1000m) * 1000m,
            selectedCoverages:   selectedCoverages,
            appliedMultipliers:  multipliers.AsReadOnly(),
            finalMonthlyPremium: finalPremium);
    }

    /// <summary>
    /// Crea el snapshot de selección para persistir en la póliza.
    /// </summary>
    public static HomePlanSelection Select(string? packageId, HomeQuotation quotation)
    {
        var package    = packageId is not null ? HomePlanPackageCatalog.FindById(packageId) : null;
        var packageName = package?.Name ?? "Personalizado";

        return new HomePlanSelection(
            packageId:           packageId,
            packageName:         packageName,
            propertyValue:       quotation.PropertyValue,
            constructionYear:    quotation.ConstructionYear,
            stratum:             quotation.Stratum,
            occupants:           quotation.Occupants,
            propertyType:        quotation.PropertyType,
            selectedCoverages:   quotation.SelectedCoverages,
            appliedMultipliers:  quotation.AppliedMultipliers,
            baseMonthlyPremium:  quotation.BaseMonthlyPremium,
            finalMonthlyPremium: quotation.FinalMonthlyPremium);
    }
}
