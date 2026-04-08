namespace InsuraTech.Domain.Tests.Policies;

using FluentAssertions;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.HealthPlan;

public sealed class HealthPlanPricingServiceTests
{
    private static readonly DateOnly Today = new(2026, 4, 6);

    // ─── Helper ────────────────────────────────────────────────────────────────

    private static DateOnly BirthDateForAge(int age) =>
        Today.AddYears(-age);

    // ─── Catálogo ──────────────────────────────────────────────────────────────

    [Fact]
    public void HealthPlanCatalog_FindById_Basic_ReturnsCorrectPlan()
    {
        var plan = HealthPlanCatalog.FindById("basic");
        plan.Should().NotBeNull();
        plan!.BaseAmount.Should().Be(300_000m);
    }

    [Fact]
    public void HealthPlanCatalog_FindById_SaludGlobal_ReturnsCorrectPlan()
    {
        var plan = HealthPlanCatalog.FindById("salud-global");
        plan.Should().NotBeNull();
        plan!.BaseAmount.Should().Be(380_000m);
    }

    [Fact]
    public void HealthPlanCatalog_FindById_SaludPremium_ReturnsCorrectPlan()
    {
        var plan = HealthPlanCatalog.FindById("salud-premium");
        plan.Should().NotBeNull();
        plan!.BaseAmount.Should().Be(450_000m);
    }

    [Fact]
    public void HealthPlanCatalog_FindById_SaludVidaTotal_ReturnsCorrectPlan()
    {
        var plan = HealthPlanCatalog.FindById("salud-vida-total");
        plan.Should().NotBeNull();
        plan!.BaseAmount.Should().Be(600_000m);
    }

    [Fact]
    public void HealthPlanCatalog_FindById_InvalidId_ReturnsNull()
    {
        var plan = HealthPlanCatalog.FindById("plan-inexistente");
        plan.Should().BeNull();
    }

    // ─── Rango 18–35: sin incremento ──────────────────────────────────────────

    [Theory]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(35)]
    public void Calculate_AgeInRange18To35_ReturnsBaseAmount(int age)
    {
        // GIVEN
        var birthDate = BirthDateForAge(age);

        // WHEN
        var result = HealthPlanPricingService.Calculate("basic", birthDate, Today);

        // THEN
        result.AgeFactorPercentage.Should().Be(0);
        result.AgeFactorAmount.Should().Be(0m);
        result.FinalAmount.Should().Be(300_000m);
    }

    // ─── Rango 36–58: incremento 4% ───────────────────────────────────────────

    [Theory]
    [InlineData(36)]
    [InlineData(45)]
    [InlineData(58)]
    public void Calculate_AgeInRange36To58_Returns4PercentIncrement(int age)
    {
        // GIVEN
        var birthDate = BirthDateForAge(age);

        // WHEN
        var result = HealthPlanPricingService.Calculate("salud-premium", birthDate, Today);

        // THEN
        result.AgeFactorPercentage.Should().Be(4);
        result.AgeFactorAmount.Should().Be(18_000m);   // 450_000 * 4% = 18_000
        result.FinalAmount.Should().Be(468_000m);
    }

    // ─── Rango 59–73: incremento 8% ───────────────────────────────────────────

    [Theory]
    [InlineData(59)]
    [InlineData(67)]
    [InlineData(73)]
    public void Calculate_AgeInRange59To73_Returns8PercentIncrement(int age)
    {
        // GIVEN
        var birthDate = BirthDateForAge(age);

        // WHEN
        var result = HealthPlanPricingService.Calculate("salud-vida-total", birthDate, Today);

        // THEN
        result.AgeFactorPercentage.Should().Be(8);
        result.AgeFactorAmount.Should().Be(48_000m);   // 600_000 * 8% = 48_000
        result.FinalAmount.Should().Be(648_000m);
    }

    // ─── Bloqueo ≥ 74 ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(74)]
    [InlineData(80)]
    [InlineData(90)]
    public void Calculate_AgeEqualOrAbove74_ThrowsOverageInsuredException(int age)
    {
        // GIVEN
        var birthDate = BirthDateForAge(age);

        // WHEN
        var act = () => HealthPlanPricingService.Calculate("basic", birthDate, Today);

        // THEN
        act.Should().Throw<OverageInsuredException>();
    }

    // ─── Menor de 18 ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(17)]
    [InlineData(0)]
    public void Calculate_AgeBellow18_ThrowsUnderageInsuredException(int age)
    {
        // GIVEN
        var birthDate = BirthDateForAge(age);

        // WHEN
        var act = () => HealthPlanPricingService.Calculate("basic", birthDate, Today);

        // THEN
        act.Should().Throw<UnderageInsuredException>();
    }

    // ─── Plan inválido ─────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_InvalidPlanId_ThrowsInvalidHealthPlanException()
    {
        // GIVEN
        var birthDate = BirthDateForAge(30);

        // WHEN
        var act = () => HealthPlanPricingService.Calculate("plan-xyz", birthDate, Today);

        // THEN
        act.Should().Throw<InvalidHealthPlanException>()
            .WithMessage("*plan-xyz*");
    }

    // ─── Redondeo ─────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_SaludGlobal_Age40_RoundsCorrectly()
    {
        // GIVEN — Salud Global $380.000 * 4% = $15.200 (exacto, sin redondeo)
        var birthDate = BirthDateForAge(40);

        // WHEN
        var result = HealthPlanPricingService.Calculate("salud-global", birthDate, Today);

        // THEN
        result.AgeFactorAmount.Should().Be(15_200m);
        result.FinalAmount.Should().Be(395_200m);
    }

    // ─── Persistencia en Policy ────────────────────────────────────────────────

    [Fact]
    public void Calculate_ReturnsSelectionWithAllFields()
    {
        // GIVEN
        var birthDate = BirthDateForAge(40);

        // WHEN
        var result = HealthPlanPricingService.Calculate("salud-premium", birthDate, Today);

        // THEN
        result.PlanId.Should().Be("salud-premium");
        result.PlanName.Should().Be("Salud Premium");
        result.BaseAmount.Should().Be(450_000m);
        result.AgeFactorPercentage.Should().Be(4);
        result.FinalAmount.Should().Be(468_000m);
    }
}
