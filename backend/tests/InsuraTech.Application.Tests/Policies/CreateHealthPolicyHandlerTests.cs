namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using NSubstitute;

public sealed class CreateHealthPolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreatePolicyHandler _handler;

    public CreateHealthPolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreatePolicyHandler(_policyRepository, _unitOfWork);

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);
    }

    private static CreatePolicyCommand HealthCommand(DateOnly? birthDate = null, string planId = "salud-premium") =>
        new()
        {
            IdempotencyKey    = Guid.NewGuid().ToString(),
            Type              = PolicyType.Health,
            InsuredFullName   = "Maria García",
            InsuredDocumentId = "987654321",
            InsuredBirthDate  = birthDate ?? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-40),
            CoverageStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CoverageEndDate   = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1),
            MonthlyPremium    = 5_000m,   // < 5% de cualquier plan (min base: $300.000 → max prima $15.000)
            HealthPlanId      = planId
        };

    [Fact]
    public async Task Handle_HealthPolicy_ValidPlanAndAge_CreatesPolicy()
    {
        // GIVEN
        var command = HealthCommand();

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Type.Should().Be("Health");
        result.Status.Should().Be("Pending");
        result.HealthPlan.Should().NotBeNull();
        result.HealthPlan!.PlanId.Should().Be("salud-premium");
        result.InsuredAmount.Should().Be(468_000m);   // 450_000 * 1.04
    }

    [Fact]
    public async Task Handle_HealthPolicy_InsuredAmountEqualsCalculatedFinalAmount()
    {
        // GIVEN — asegurado de 25 años (0% increment) con plan básico
        var command = HealthCommand(
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-25),
            planId: "basic");

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.InsuredAmount.Should().Be(300_000m);
        result.HealthPlan!.AgeFactorPercentage.Should().Be(0);
    }

    [Fact]
    public async Task Handle_HealthPolicy_Age74_ThrowsOverageInsuredException()
    {
        // GIVEN
        var command = HealthCommand(birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-74));

        // WHEN
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<OverageInsuredException>();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HealthPolicy_InvalidPlanId_ThrowsInvalidHealthPlanException()
    {
        // GIVEN
        var command = HealthCommand(planId: "plan-invalido");

        // WHEN
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidHealthPlanException>();
    }

    [Fact]
    public async Task Handle_NonHealthPolicy_IgnoresHealthPlanId()
    {
        // GIVEN — tipo Life con HealthPlanId presente (no debe usarse)
        var command = new CreatePolicyCommand
        {
            IdempotencyKey    = Guid.NewGuid().ToString(),
            Type              = PolicyType.Life,
            InsuredFullName   = "Juan Pérez",
            InsuredDocumentId = "111222333",
            InsuredBirthDate  = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-35),
            CoverageStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CoverageEndDate   = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1),
            MonthlyPremium    = 200m,
            InsuredAmount     = 10_000m,
            HealthPlanId      = "salud-premium"   // debe ignorarse
        };

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Type.Should().Be("Life");
        result.HealthPlan.Should().BeNull();
        result.InsuredAmount.Should().Be(10_000m);
    }
}
