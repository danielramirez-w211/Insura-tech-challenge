namespace InsuraTech.Application.Tests.TravelPlans;

using FluentAssertions;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.TravelPlans.Queries.CalculateTravelPlan;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.TravelPlan;
using NSubstitute;

public sealed class CalculateTravelPlanHandlerTests
{
    private readonly ITrmService _trmService;
    private readonly CalculateTravelPlanHandler _handler;

    private static readonly TrmResult FakeTrm = new(4_200m, new DateOnly(2026, 4, 7));

    public CalculateTravelPlanHandlerTests()
    {
        _trmService = Substitute.For<ITrmService>();
        _handler    = new CalculateTravelPlanHandler(_trmService);
    }

    [Fact]
    public async Task Handle_Nacional_ReturnsCorrectCalculation()
    {
        // GIVEN
        var query = new CalculateTravelPlanQuery
        {
            TripType     = TripType.Nacional,
            DurationDays = 10
        };

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.TripType.Should().Be("Nacional");
        result.TotalPriceCop.Should().Be(13_000m); // 2200 + 9*1200
        result.TrmUsed.Should().BeNull();
        await _trmService.DidNotReceive().GetCurrentTrmAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Internacional_FetchesTrmAndCalculates()
    {
        // GIVEN
        _trmService.GetCurrentTrmAsync(Arg.Any<CancellationToken>()).Returns(FakeTrm);

        var query = new CalculateTravelPlanQuery
        {
            TripType     = TripType.Internacional,
            Continent    = Continent.Europe,
            DurationDays = 10
        };

        // WHEN
        var result = await _handler.Handle(query, CancellationToken.None);

        // THEN
        result.TripType.Should().Be("Internacional");
        result.Continent.Should().Be("Europe");
        result.TotalPriceCop.Should().Be(315_000m); // 75 USD × 4200
        result.TrmUsed.Should().Be(4_200m);
        await _trmService.Received(1).GetCurrentTrmAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TrmUnavailable_PropagatesException()
    {
        // GIVEN
        _trmService.GetCurrentTrmAsync(Arg.Any<CancellationToken>())
            .Returns<TrmResult>(_ => throw new TrmUnavailableException());

        var query = new CalculateTravelPlanQuery
        {
            TripType     = TripType.Internacional,
            Continent    = Continent.Asia,
            DurationDays = 5
        };

        // WHEN
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<TrmUnavailableException>();
    }

    [Fact]
    public async Task Handle_DurationExceeded_PropagatesException()
    {
        // GIVEN
        var query = new CalculateTravelPlanQuery
        {
            TripType     = TripType.Nacional,
            DurationDays = 181
        };

        // WHEN
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<TravelDurationExceededException>();
    }
}
