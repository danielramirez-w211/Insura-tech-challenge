namespace InsuraTech.Application.Tests.HealthPlans;

using FluentAssertions;
using InsuraTech.Application.HealthPlans.Queries.GetHealthPlans;

public sealed class GetHealthPlansHandlerTests
{
    private readonly GetHealthPlansHandler _handler = new();

    [Fact]
    public async Task Handle_ReturnsAll4Plans()
    {
        // GIVEN
        var query = new GetHealthPlansQuery();

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_ContainsBasicPlan()
    {
        // WHEN
        var result = await _handler.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        // THEN
        result.Should().Contain(p => p.PlanId == "basic" && p.BaseAmount == 300_000m);
    }

    [Fact]
    public async Task Handle_ContainsSaludVidaTotalPlan()
    {
        // WHEN
        var result = await _handler.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        // THEN
        result.Should().Contain(p => p.PlanId == "salud-vida-total" && p.BaseAmount == 600_000m);
    }

    [Fact]
    public async Task Handle_AllPlansHaveNonEmptyNames()
    {
        // WHEN
        var result = await _handler.Handle(new GetHealthPlansQuery(), CancellationToken.None);

        // THEN
        result.Should().AllSatisfy(p => p.PlanName.Should().NotBeNullOrWhiteSpace());
    }
}
