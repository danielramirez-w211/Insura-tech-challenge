namespace InsuraTech.Application.Tests.HealthPlans;

using FluentAssertions;
using InsuraTech.Application.HealthPlans.Queries.CalculateHealthPlan;
using InsuraTech.Domain.Exceptions;

public sealed class CalculateHealthPlanHandlerTests
{
    private readonly CalculateHealthPlanHandler _handler = new();

    [Fact]
    public async Task Handle_ValidPlanAndAge_ReturnsCorrectAmounts()
    {
        // GIVEN — asegurado de 40 años → rango 36-58 → 4%
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-40);
        var query = new CalculateHealthPlanQuery
        {
            PlanId    = "salud-premium",
            BirthDate = birthDate
        };

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.BaseAmount.Should().Be(450_000m);
        result.AgeFactorPercentage.Should().Be(4);
        result.FinalAmount.Should().Be(468_000m);
        result.InsuredAge.Should().Be(40);
    }

    [Fact]
    public async Task Handle_Age25_ReturnsNoIncrement()
    {
        // GIVEN — asegurado de 25 años → rango 18-35 → 0%
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-25);
        var query = new CalculateHealthPlanQuery
        {
            PlanId    = "basic",
            BirthDate = birthDate
        };

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.AgeFactorPercentage.Should().Be(0);
        result.FinalAmount.Should().Be(300_000m);
    }

    [Fact]
    public async Task Handle_Age74_ThrowsOverageInsuredException()
    {
        // GIVEN
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-74);
        var query = new CalculateHealthPlanQuery
        {
            PlanId    = "basic",
            BirthDate = birthDate
        };

        // WHEN
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<OverageInsuredException>();
    }

    [Fact]
    public async Task Handle_InvalidPlanId_ThrowsInvalidHealthPlanException()
    {
        // GIVEN
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30);
        var query = new CalculateHealthPlanQuery
        {
            PlanId    = "plan-no-existe",
            BirthDate = birthDate
        };

        // WHEN
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidHealthPlanException>();
    }
}
