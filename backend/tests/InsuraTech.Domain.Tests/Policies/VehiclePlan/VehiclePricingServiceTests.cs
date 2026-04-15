namespace InsuraTech.Domain.Tests.Policies.VehiclePlan;

using FluentAssertions;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.VehiclePlan;

/// <summary>
/// Tests para VehiclePricingService (SPEC-010).
/// Todos los valores esperados se derivan de la tabla de verificación del spec (sección 1.4).
/// </summary>
public sealed class VehiclePricingServiceTests
{
    private const int CurrentYear = 2026;

    // ─── Tasa técnica por antigüedad (RN-01, RN-09) ───────────────────────────

    [Fact]
    public void Calculate_NewVehicle_AppliesRate2_2Percent()
    {
        // GIVEN — vehículo del año actual (antigüedad = 0)
        // WHEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // THEN
        result.AgeCategory.Should().Be("Nuevo");
        result.TechnicalRate.Should().Be(0.022m);
        result.VehicleAge.Should().Be(0);
        result.AnnualBase.Should().Be(1_100_000m);
        result.BaseMonthlyPremium.Should().Be(91_667m);
    }

    [Fact]
    public void Calculate_UsedRecentVehicle_AppliesRate2_8Percent()
    {
        // GIVEN — vehículo de 3 años (antigüedad 1–5)
        // WHEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2023, "Toyota", CurrentYear);

        // THEN
        result.AgeCategory.Should().Be("Usado Reciente");
        result.TechnicalRate.Should().Be(0.028m);
        result.VehicleAge.Should().Be(3);
        result.AnnualBase.Should().Be(1_400_000m);
        result.BaseMonthlyPremium.Should().Be(116_667m);
    }

    [Fact]
    public void Calculate_UsedOldVehicle_AppliesRate3_5Percent()
    {
        // GIVEN — vehículo de 8 años (antigüedad > 5)
        // WHEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2018, "Toyota", CurrentYear);

        // THEN
        result.AgeCategory.Should().Be("Usado Antiguo");
        result.TechnicalRate.Should().Be(0.035m);
        result.VehicleAge.Should().Be(8);
        result.AnnualBase.Should().Be(1_750_000m);
        result.BaseMonthlyPremium.Should().Be(145_833m);
    }

    // ─── Recargo por marca (RN-03) ────────────────────────────────────────────

    [Fact]
    public void Calculate_HighSinistrabilityBrand_Applies10PercentSurcharge()
    {
        // GIVEN — Renault: $30M × 2.8% × 1.10 = $924.000
        // WHEN
        var result = VehiclePricingService.Calculate(30_000_000m, 2023, "Renault", CurrentYear);

        // THEN
        result.HasBrandSurcharge.Should().BeTrue();
        result.AnnualBase.Should().Be(924_000m);
        result.BaseMonthlyPremium.Should().Be(77_000m);
    }

    [Fact]
    public void Calculate_NormalBrand_DoesNotApplySurcharge()
    {
        // GIVEN
        // WHEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // THEN
        result.HasBrandSurcharge.Should().BeFalse();
    }

    // ─── Prima mínima (RN-04) ────────────────────────────────────────────────

    [Fact]
    public void Calculate_LowCommercialValue_EnforcesMinimumPremium()
    {
        // GIVEN — $10M × 2.2% = $220.000 → aplica mínimo $800.000
        // WHEN
        var result = VehiclePricingService.Calculate(10_000_000m, 2026, "Toyota", CurrentYear);

        // THEN
        result.AnnualBase.Should().Be(800_000m);
        result.BaseMonthlyPremium.Should().Be(66_667m);
    }

    // ─── Redondeo al mil superior (RN-05) ────────────────────────────────────

    [Fact]
    public void Calculate_PremiumPlan_CeilingRoundingApplied()
    {
        // GIVEN — $50M × 2.2% × 1.45 = $1.595.000 (exacto, sin redondeo real)
        // $135M × 2.2% × 1.45 = $4.306.500 → CEILING = $4.307.000
        // WHEN
        var result = VehiclePricingService.Calculate(135_000_000m, 2026, "Toyota", CurrentYear);

        // THEN — anualPremium = CEILING(4,306,500 / 1000) × 1000 = 4,307,000
        result.AnnualPremium.Should().Be(4_307_000m);
        result.PremiumPlanPremium.Should().Be(358_917m);
    }

    [Fact]
    public void Calculate_CompletePlan_CeilingRoundingApplied()
    {
        // GIVEN — Renault $30M: annualBase=924,000 × 1.20 = 1,108,800 → CEILING = 1,109,000
        // WHEN
        var result = VehiclePricingService.Calculate(30_000_000m, 2023, "Renault", CurrentYear);

        // THEN
        result.AnnualComplete.Should().Be(1_109_000m);
        result.CompletePlanPremium.Should().Be(92_417m);
    }

    // ─── Multiplicadores por plan (RN-06, RN-07) ────────────────────────────

    [Fact]
    public void Calculate_CompletePlan_Is1_20xAnnualBase()
    {
        // GIVEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // THEN — 1,100,000 × 1.20 = 1,320,000
        result.AnnualComplete.Should().Be(1_320_000m);
        result.CompletePlanPremium.Should().Be(110_000m);
    }

    [Fact]
    public void Calculate_PremiumPlan_Is1_45xAnnualBase()
    {
        // GIVEN
        var result = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // THEN — 1,100,000 × 1.45 = 1,595,000
        result.AnnualPremium.Should().Be(1_595_000m);
        result.PremiumPlanPremium.Should().Be(132_917m);
    }

    // ─── Prima mensual = prima anual / 12 (RN-18) ──────────────────────────

    [Fact]
    public void Calculate_AllPlans_MonthlyPremiumIsAnnualDividedBy12()
    {
        // GIVEN
        var result = VehiclePricingService.Calculate(135_000_000m, 2026, "Toyota", CurrentYear);

        // THEN — spec table row: $135M, 0 años, no surcharge
        result.BaseMonthlyPremium.Should().Be(247_500m);   // 2,970,000 / 12
        result.CompletePlanPremium.Should().Be(297_000m);  // 3,564,000 / 12
        result.PremiumPlanPremium.Should().Be(358_917m);   // 4,307,000 / 12 ≈ 358,916.67 → 358,917
    }

    // ─── Descuento anual 5% sobre Select (RN-08) ────────────────────────────

    [Fact]
    public void Select_StandardPlan_AnnualWithDiscountIs5PercentOffAnnualBase()
    {
        // GIVEN
        var quotation = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // WHEN
        var selection = VehiclePricingService.Select("standard", quotation);

        // THEN — 1,100,000 × 0.95 = 1,045,000
        selection.AnnualPremiumWithDiscount.Should().Be(1_045_000m);
        selection.FinalMonthlyPremium.Should().Be(91_667m);
    }

    [Fact]
    public void Select_CompletePlan_AnnualWithDiscountIs5PercentOffAnnualComplete()
    {
        // GIVEN
        var quotation = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // WHEN
        var selection = VehiclePricingService.Select("complete", quotation);

        // THEN — 1,320,000 × 0.95 = 1,254,000
        selection.AnnualPremiumWithDiscount.Should().Be(1_254_000m);
        selection.FinalMonthlyPremium.Should().Be(110_000m);
    }

    [Fact]
    public void Select_PremiumPlan_AnnualWithDiscountIs5PercentOffAnnualPremium()
    {
        // GIVEN
        var quotation = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);

        // WHEN
        var selection = VehiclePricingService.Select("premium", quotation);

        // THEN — 1,595,000 × 0.95 = 1,515,250
        selection.AnnualPremiumWithDiscount.Should().Be(1_515_250m);
        selection.FinalMonthlyPremium.Should().Be(132_917m);
    }

    [Fact]
    public void Select_AnnualPremiumWithDiscount_IsAlwaysGreaterThanMonthlyPremium()
    {
        // GIVEN — verificar invariante RN-18: anual con descuento > mensual
        var quotation = VehiclePricingService.Calculate(135_000_000m, 2026, "Toyota", CurrentYear);

        foreach (var planId in new[] { "standard", "complete", "premium" })
        {
            // WHEN
            var selection = VehiclePricingService.Select(planId, quotation);

            // THEN
            selection.AnnualPremiumWithDiscount.Should()
                .BeGreaterThan(selection.FinalMonthlyPremium,
                    because: $"plan '{planId}': anual con descuento debe ser mayor que la prima mensual");
        }
    }

    // ─── Catálogo (FindById) ────────────────────────────────────────────────

    [Fact]
    public void VehiclePlanCatalog_FindById_Standard_ReturnsPlan()
    {
        var plan = VehiclePlanCatalog.FindById("standard");
        plan.Should().NotBeNull();
        plan!.Name.Should().Be("Plan Estándar");
        plan.PriceMultiplier.Should().Be(1.00m);
    }

    [Fact]
    public void VehiclePlanCatalog_FindById_Complete_ReturnsPlan()
    {
        var plan = VehiclePlanCatalog.FindById("complete");
        plan.Should().NotBeNull();
        plan!.Name.Should().Be("Plan Completo");
        plan.PriceMultiplier.Should().Be(1.20m);
    }

    [Fact]
    public void VehiclePlanCatalog_FindById_Premium_ReturnsPlan()
    {
        var plan = VehiclePlanCatalog.FindById("premium");
        plan.Should().NotBeNull();
        plan!.Name.Should().Be("Plan Premium");
        plan.PriceMultiplier.Should().Be(1.45m);
    }

    [Fact]
    public void VehiclePlanCatalog_FindById_Invalid_ReturnsNull()
    {
        var plan = VehiclePlanCatalog.FindById("plan-inexistente");
        plan.Should().BeNull();
    }

    // ─── Cobertura acumulativa (RN-17) ──────────────────────────────────────

    [Fact]
    public void VehiclePlanCatalog_CompletePlan_ContainsAllStandardCoverages()
    {
        var standard = VehiclePlanCatalog.Standard;
        var complete  = VehiclePlanCatalog.Complete;

        foreach (var coverage in standard.Coverages)
            complete.Coverages.Should().Contain(coverage,
                because: $"Plan Completo debe incluir cobertura '{coverage}' del Plan Estándar");
    }

    [Fact]
    public void VehiclePlanCatalog_PremiumPlan_ContainsAllCompleteCoverages()
    {
        var complete = VehiclePlanCatalog.Complete;
        var premium  = VehiclePlanCatalog.Premium;

        foreach (var coverage in complete.Coverages)
            premium.Coverages.Should().Contain(coverage,
                because: $"Plan Premium debe incluir cobertura '{coverage}' del Plan Completo");
    }

    // ─── Validaciones de entrada ────────────────────────────────────────────

    [Fact]
    public void Calculate_ZeroCommercialValue_ThrowsArgumentException()
    {
        var act = () => VehiclePricingService.Calculate(0m, 2026, "Toyota", CurrentYear);
        act.Should().Throw<ArgumentException>().WithMessage("*valor comercial*");
    }

    [Fact]
    public void Calculate_NegativeCommercialValue_ThrowsArgumentException()
    {
        var act = () => VehiclePricingService.Calculate(-1m, 2026, "Toyota", CurrentYear);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calculate_FutureVehicleYear_ThrowsArgumentException()
    {
        var act = () => VehiclePricingService.Calculate(50_000_000m, CurrentYear + 1, "Toyota", CurrentYear);
        act.Should().Throw<ArgumentException>().WithMessage($"*{CurrentYear}*");
    }

    [Fact]
    public void Calculate_VehicleYearBefore1900_ThrowsArgumentException()
    {
        var act = () => VehiclePricingService.Calculate(50_000_000m, 1899, "Toyota", CurrentYear);
        act.Should().Throw<ArgumentException>().WithMessage("*1900*");
    }

    [Fact]
    public void Select_InvalidPlanId_ThrowsInvalidVehiclePlanException()
    {
        var quotation = VehiclePricingService.Calculate(50_000_000m, 2026, "Toyota", CurrentYear);
        var act = () => VehiclePricingService.Select("plan-invalido", quotation);
        act.Should().Throw<InvalidVehiclePlanException>();
    }

    // ─── Snapshot de selección — campos correctos ────────────────────────────

    [Fact]
    public void Select_ReturnsSelectionWithAllFields()
    {
        // GIVEN
        var quotation = VehiclePricingService.Calculate(50_000_000m, 2023, "Toyota", CurrentYear);

        // WHEN
        var selection = VehiclePricingService.Select("complete", quotation);

        // THEN
        selection.PlanId.Should().Be("complete");
        selection.PlanName.Should().Be("Plan Completo");
        selection.VehicleBrand.Should().Be("Toyota");
        selection.VehicleYear.Should().Be(2023);
        selection.CommercialValue.Should().Be(50_000_000m);
        selection.Coverages.Should().NotBeEmpty();
        selection.Assistances.Should().Contain("Grúa");
    }
}
