namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.TravelPlan;
using NSubstitute;

public sealed class CreateTravelPolicyHandlerTests
{
    private readonly IPolicyRepository   _policyRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ITrmService         _trmService;
    private readonly CreatePolicyHandler _handler;

    private static readonly TrmResult FakeTrm = new(4_200m, new DateOnly(2026, 4, 7));

    public CreateTravelPolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork       = Substitute.For<IUnitOfWork>();
        _trmService       = Substitute.For<ITrmService>();

        var strategies = new ICreatePolicyStrategy[]
        {
            new CreateHealthPolicyStrategy(),
            new CreateLifePolicyStrategy(),
            new CreateVehiclePolicyStrategy(),
            new CreateHomePolicyStrategy(),
            new CreateTravelPolicyStrategy(_trmService),
        };

        _handler = new CreatePolicyHandler(_policyRepository, _unitOfWork, strategies);

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);
    }

    private static CreatePolicyCommand NacionalCommand(int days = 10) => new()
    {
        IdempotencyKey    = Guid.NewGuid().ToString(),
        Type              = PolicyType.Travel,
        InsuredFirstName = "Carlos", InsuredLastName = "Viajero", InsuredDocumentType = "CC",
        InsuredDocumentId = "555666777",
        InsuredBirthDate  = new DateOnly(1990, 6, 15),
        CoverageStartDate = new DateOnly(2026, 5, 1),
        CoverageEndDate   = new DateOnly(2026, 5, 1).AddDays(days),
        TripType          = TripType.Nacional,
        DurationDays      = days
    };

    private static CreatePolicyCommand InternacionalCommand(
        int days = 10, Continent continent = Continent.Europe) => new()
    {
        IdempotencyKey    = Guid.NewGuid().ToString(),
        Type              = PolicyType.Travel,
        InsuredFirstName = "Laura", InsuredLastName = "Internacional", InsuredDocumentType = "CC",
        InsuredDocumentId = "888999000",
        InsuredBirthDate  = new DateOnly(1985, 3, 20),
        CoverageStartDate = new DateOnly(2026, 6, 1),
        CoverageEndDate   = new DateOnly(2026, 6, 1).AddDays(days),
        TripType          = TripType.Internacional,
        Continent         = continent,
        DurationDays      = days
    };

    [Fact]
    public async Task Handle_TravelNacional_CreatesPolicy_WithCorrectTotal()
    {
        // GIVEN — 10 días nacional = 2200 + 9*1200 = 13,000
        var command = NacionalCommand(10);

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Type.Should().Be("Travel");
        result.Status.Should().Be("Pending");
        result.TravelPlan.Should().NotBeNull();
        result.TravelPlan!.TripType.Should().Be("Nacional");
        result.TravelPlan.TotalPriceCop.Should().Be(13_000m);
        result.TravelPlan.TrmUsed.Should().BeNull();
        result.InsuredAmount.Should().Be(13_000m);
    }

    [Fact]
    public async Task Handle_TravelInternacional_FetchesTrm_AndCreatesPolicy()
    {
        // GIVEN
        _trmService.GetCurrentTrmAsync(Arg.Any<CancellationToken>()).Returns(FakeTrm);
        var command = InternacionalCommand(10, Continent.Europe);

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.TravelPlan.Should().NotBeNull();
        result.TravelPlan!.TripType.Should().Be("Internacional");
        result.TravelPlan.Continent.Should().Be("Europe");
        result.TravelPlan.TotalPriceCop.Should().Be(315_000m);
        result.TravelPlan.TrmUsed.Should().Be(4_200m);
        await _trmService.Received(1).GetCurrentTrmAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TrmUnavailable_ThrowsAndDoesNotPersist()
    {
        // GIVEN
        _trmService.GetCurrentTrmAsync(Arg.Any<CancellationToken>())
            .Returns<TrmResult>(_ => throw new TrmUnavailableException());

        // WHEN
        var act = async () => await _handler.Handle(
            InternacionalCommand(), CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<TrmUnavailableException>();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DurationExceeded_ThrowsAndDoesNotPersist()
    {
        // GIVEN — 181 días supera el máximo
        var command = NacionalCommand(181);

        // WHEN
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<TravelDurationExceededException>();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonTravelPolicy_DoesNotCallTrmService()
    {
        // GIVEN — póliza Life (no Travel)
        var command = new CreatePolicyCommand
        {
            IdempotencyKey    = Guid.NewGuid().ToString(),
            Type              = PolicyType.Life,
            InsuredFirstName = "Juan", InsuredLastName = "Vida", InsuredDocumentType = "CC",
            InsuredDocumentId = "111222333",
            InsuredBirthDate  = new DateOnly(1985, 1, 1),
            CoverageStartDate = new DateOnly(2026, 1, 1),
            CoverageEndDate   = new DateOnly(2027, 1, 1),
            MonthlyPremium    = 200m,
            InsuredAmount     = 10_000m
        };

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Type.Should().Be("Life");
        result.TravelPlan.Should().BeNull();
        await _trmService.DidNotReceive().GetCurrentTrmAsync(Arg.Any<CancellationToken>());
    }
}
