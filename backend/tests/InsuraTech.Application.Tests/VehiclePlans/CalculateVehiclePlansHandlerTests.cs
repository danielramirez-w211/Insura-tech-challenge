namespace InsuraTech.Application.Tests.VehiclePlans;

using FluentAssertions;
using InsuraTech.Application.VehiclePlans.Queries.CalculateVehiclePlans;

/// <summary>
/// Tests para CalculateVehiclePlansHandler (SPEC-010).
/// El handler no tiene dependencias externas — sin mocks necesarios.
/// </summary>
public sealed class CalculateVehiclePlansHandlerTests
{
    private readonly CalculateVehiclePlansHandler _handler = new();

    private static CalculateVehiclePlansQuery QueryFor(
        decimal commercialValue, int vehicleYear, string brand) => new()
    {
        CommercialValue = commercialValue,
        VehicleYear     = vehicleYear,
        Brand           = brand,
    };

    // ─── Retorna los tres planes ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsThreePlans_WithCorrectIds()
    {
        // GIVEN
        var query = QueryFor(50_000_000m, 2026, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.Plans.Should().HaveCount(3);
        result.Plans.Select(p => p.PlanId)
            .Should().BeEquivalentTo(["standard", "complete", "premium"]);
    }

    // ─── Primas mensuales correctas (tabla spec §1.4) ─────────────────────────

    [Fact]
    public async Task Handle_NewVehicle50M_ReturnsCorrectMonthlyPremiums()
    {
        // GIVEN — $50M, año 2026 (nuevo), Toyota (sin recargo)
        var query = QueryFor(50_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        var std  = result.Plans.Single(p => p.PlanId == "standard");
        var comp = result.Plans.Single(p => p.PlanId == "complete");
        var prem = result.Plans.Single(p => p.PlanId == "premium");

        std.MonthlyPremium.Should().Be(91_667m);
        comp.MonthlyPremium.Should().Be(110_000m);
        prem.MonthlyPremium.Should().Be(132_917m);
    }

    // ─── AnnualPremiumWithDiscount = prima anual × 0.95 (RN-08) ─────────────

    [Fact]
    public async Task Handle_AnnualPremiumWithDiscount_Is5PercentOffAnnualPremium()
    {
        // GIVEN — $50M, año 2026, Toyota
        var query = QueryFor(50_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN — Standard: 1,100,000 × 0.95 = 1,045,000
        var std = result.Plans.Single(p => p.PlanId == "standard");
        std.AnnualPremiumWithDiscount.Should().Be(1_045_000m);

        // Complete: 1,320,000 × 0.95 = 1,254,000
        var comp = result.Plans.Single(p => p.PlanId == "complete");
        comp.AnnualPremiumWithDiscount.Should().Be(1_254_000m);

        // Premium: 1,595,000 × 0.95 = 1,515,250
        var prem = result.Plans.Single(p => p.PlanId == "premium");
        prem.AnnualPremiumWithDiscount.Should().Be(1_515_250m);
    }

    [Fact]
    public async Task Handle_AnnualPremiumWithDiscount_IsAlwaysGreaterThanMonthlyPremium()
    {
        // GIVEN — invariante RN-18
        var query = QueryFor(135_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        foreach (var plan in result.Plans)
            plan.AnnualPremiumWithDiscount.Should()
                .BeGreaterThan(plan.MonthlyPremium,
                    because: $"plan '{plan.PlanId}': anual con descuento debe ser mayor que prima mensual");
    }

    // ─── Recargo por marca (RN-03) ────────────────────────────────────────────

    [Fact]
    public async Task Handle_HighSinistrabilityBrand_ReturnsHigherPremiums()
    {
        // GIVEN
        var normalQuery   = QueryFor(30_000_000m, 2023, "Toyota");
        var surchargeQuery = QueryFor(30_000_000m, 2023, "Renault");

        // WHEN
        var normalResult   = await _handler.Handle(normalQuery,   CancellationToken.None);
        var surchargeResult = await _handler.Handle(surchargeQuery, CancellationToken.None);

        // THEN
        var normalStd   = normalResult.Plans.Single(p => p.PlanId == "standard").MonthlyPremium;
        var surchargeStd = surchargeResult.Plans.Single(p => p.PlanId == "standard").MonthlyPremium;

        surchargeStd.Should().BeGreaterThan(normalStd);
        surchargeResult.HasBrandSurcharge.Should().BeTrue();
        normalResult.HasBrandSurcharge.Should().BeFalse();
    }

    // ─── Campos de cabecera del resultado ────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsCorrectQuotationHeader()
    {
        // GIVEN
        var query = QueryFor(50_000_000m, 2023, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.CommercialValue.Should().Be(50_000_000m);
        result.Brand.Should().Be("Toyota");
        result.AgeCategory.Should().Be("Usado Reciente");
        result.TechnicalRate.Should().Be(0.028m);
        result.BaseMonthlyPremium.Should().Be(116_667m);
    }

    // ─── Coberturas y asistencias en la respuesta ─────────────────────────────

    [Fact]
    public async Task Handle_StandardPlan_HasNoCoverageAssistances()
    {
        // GIVEN
        var query = QueryFor(50_000_000m, 2026, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.Plans.Single(p => p.PlanId == "standard").Assistances.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PremiumPlan_HasFourAssistances()
    {
        // GIVEN
        var query = QueryFor(50_000_000m, 2026, "Toyota");

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.Plans.Single(p => p.PlanId == "premium").Assistances
            .Should().HaveCount(4);
    }
}
