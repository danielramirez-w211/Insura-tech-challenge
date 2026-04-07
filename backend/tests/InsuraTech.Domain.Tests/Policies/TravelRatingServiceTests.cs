namespace InsuraTech.Domain.Tests.Policies;

using FluentAssertions;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.TravelPlan;

public sealed class TravelRatingServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 4, 7, 0, 0, 0, DateTimeKind.Utc);
    private const decimal FixedTrm = 4_200m;
    private static readonly DateOnly FixedTrmDate = new(2026, 4, 7);

    // ─── Utilitarios ──────────────────────────────────────────────────────────

    private static TravelPlanSelection Nacional(int days) =>
        TravelRatingService.Calculate(TripType.Nacional, null, days, null, null, FixedNow);

    private static TravelPlanSelection Internacional(int days, Continent continent = Continent.Europe) =>
        TravelRatingService.Calculate(TripType.Internacional, continent, days, FixedTrm, FixedTrmDate, FixedNow);

    // ─── UT-01  Nacional día 1 ─────────────────────────────────────────────────

    [Fact]
    public void Nacional_Day1_Returns2200Cop()
    {
        var result = Nacional(1);
        result.TotalPriceCop.Should().Be(2_200m);
    }

    // ─── UT-02  Nacional 10 días ───────────────────────────────────────────────

    [Fact]
    public void Nacional_10Days_Returns13000Cop()
    {
        var result = Nacional(10);
        result.TotalPriceCop.Should().Be(13_000m); // 2200 + 9*1200
    }

    // ─── UT-03  Nacional 30 días (techo exacto) ───────────────────────────────

    [Fact]
    public void Nacional_30Days_Returns37000Cop()
    {
        var result = Nacional(30);
        result.TotalPriceCop.Should().Be(37_000m); // 2200 + 29*1200
    }

    // ─── UT-04  Nacional 45 días (sobre el techo → mismo valor que 30) ────────

    [Fact]
    public void Nacional_45Days_SameAs30Days()
    {
        var result = Nacional(45);
        result.TotalPriceCop.Should().Be(37_000m);
    }

    // ─── UT-05  Nacional 180 días (máximo permitido) ──────────────────────────

    [Fact]
    public void Nacional_180Days_SameAs30DaysCap()
    {
        var result = Nacional(180);
        result.TotalPriceCop.Should().Be(37_000m);
    }

    // ─── UT-06  Nacional 181 días → excepción ─────────────────────────────────

    [Fact]
    public void Nacional_181Days_ThrowsTravelDurationExceededException()
    {
        var act = () => Nacional(181);
        act.Should().Throw<TravelDurationExceededException>()
           .Which.Code.Should().Be("TRAVEL_DURATION_EXCEEDED");
    }

    // ─── UT-07  Nacional 0 días → excepción ───────────────────────────────────

    [Fact]
    public void Nacional_0Days_ThrowsTravelDurationInvalidException()
    {
        var act = () => Nacional(0);
        act.Should().Throw<TravelDurationInvalidException>()
           .Which.Code.Should().Be("TRAVEL_DURATION_INVALID");
    }

    // ─── UT-08  Internacional día 1 ───────────────────────────────────────────

    [Fact]
    public void Internacional_Day1_TrmFixed_Returns126000Cop()
    {
        // 30 USD × 4200 = 126,000 COP
        var result = Internacional(1);
        result.TotalPriceCop.Should().Be(126_000m);
    }

    // ─── UT-09  Internacional 10 días ─────────────────────────────────────────

    [Fact]
    public void Internacional_10Days_TrmFixed_Returns315000Cop()
    {
        // (30 + 9*5) USD = 75 USD × 4200 = 315,000
        var result = Internacional(10);
        result.TotalPriceCop.Should().Be(315_000m);
    }

    // ─── UT-10  Internacional 30 días ─────────────────────────────────────────

    [Fact]
    public void Internacional_30Days_TrmFixed_CorrectTotal()
    {
        // (30 + 29*5) = 175 USD × 4200 = 735,000
        var result = Internacional(30);
        result.TotalPriceCop.Should().Be(735_000m);
    }

    // ─── UT-11  Internacional 180 días (máximo) ───────────────────────────────

    [Fact]
    public void Internacional_180Days_TrmFixed_CorrectTotal()
    {
        // (30 + 179*5) = 925 USD × 4200 = 3,885,000
        var result = Internacional(180);
        result.TotalPriceCop.Should().Be(3_885_000m);
    }

    // ─── UT-12  Internacional 181 días → excepción ────────────────────────────

    [Fact]
    public void Internacional_181Days_ThrowsTravelDurationExceededException()
    {
        var act = () => Internacional(181);
        act.Should().Throw<TravelDurationExceededException>();
    }

    // ─── UT-13  Continente null en Internacional → excepción ──────────────────

    [Fact]
    public void Internacional_NullContinent_ThrowsInvalidContinentException()
    {
        var act = () => TravelRatingService.Calculate(
            TripType.Internacional, null, 10, FixedTrm, FixedTrmDate, FixedNow);

        act.Should().Throw<InvalidContinentException>()
           .Which.Code.Should().Be("INVALID_CONTINENT");
    }

    // ─── UT-14  TRM null en Internacional → excepción ────────────────────────

    [Fact]
    public void Internacional_NullTrm_ThrowsTrmUnavailableException()
    {
        var act = () => TravelRatingService.Calculate(
            TripType.Internacional, Continent.Asia, 10, null, null, FixedNow);

        act.Should().Throw<TrmUnavailableException>()
           .Which.Code.Should().Be("TRM_UNAVAILABLE");
    }

    // ─── UT-15  Todos los continentes calculan sin excepción (Theory) ─────────

    [Theory]
    [InlineData(Continent.America)]
    [InlineData(Continent.Europe)]
    [InlineData(Continent.Africa)]
    [InlineData(Continent.Asia)]
    [InlineData(Continent.Oceania)]
    public void Internacional_AllContinents_NoException(Continent continent)
    {
        var act = () => Internacional(10, continent);
        act.Should().NotThrow();
    }

    // ─── UT-16  Snapshot completo del VO ──────────────────────────────────────

    [Fact]
    public void Internacional_FullSnapshot_AllFieldsPopulated()
    {
        var result = Internacional(5);

        result.TripType.Should().Be(TripType.Internacional);
        result.Continent.Should().Be(Continent.Europe);
        result.DurationDays.Should().Be(5);
        result.BasePriceUsd.Should().Be(30m);
        result.TrmUsed.Should().Be(FixedTrm);
        result.TrmDate.Should().Be(FixedTrmDate);
        result.CalculatedAt.Should().Be(FixedNow);
        // (30 + 4*5) = 50 USD × 4200 = 210,000
        result.TotalPriceCop.Should().Be(210_000m);
    }

    // ─── UT-17  Incremento diario Nacional es exactamente $1,200 ─────────────

    [Fact]
    public void Nacional_DailyIncrement_IsExactly1200Cop()
    {
        var day2 = Nacional(2).TotalPriceCop;
        var day3 = Nacional(3).TotalPriceCop;
        (day3 - day2).Should().Be(1_200m);
    }

    // ─── UT-18  Incremento diario Internacional es exactamente $5 USD ─────────

    [Fact]
    public void Internacional_DailyIncrementUsd_IsExactly5Usd()
    {
        var result1 = Internacional(2);
        var result2 = Internacional(3);
        var diffUsd = (result2.TotalPriceCop - result1.TotalPriceCop) / FixedTrm;
        diffUsd.Should().Be(5m);
    }

    // ─── UT-19  Nacional TripType correcto y TRM null ─────────────────────────

    [Fact]
    public void Nacional_TripTypeAndTrmAreCorrect()
    {
        var result = Nacional(10);
        result.TripType.Should().Be(TripType.Nacional);
        result.TrmUsed.Should().BeNull();
        result.TrmDate.Should().BeNull();
        result.Continent.Should().BeNull();
        result.BasePriceUsd.Should().BeNull();
    }

    // ─── UT-20  TRM negativa → excepción ──────────────────────────────────────

    [Fact]
    public void Internacional_NegativeTrm_ThrowsTrmUnavailableException()
    {
        var act = () => TravelRatingService.Calculate(
            TripType.Internacional, Continent.Asia, 10, -100m, FixedTrmDate, FixedNow);

        act.Should().Throw<TrmUnavailableException>();
    }
}
