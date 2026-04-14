namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

/// <summary>
/// Tests de integración (Application layer) para la rama Vehicle del CreatePolicyHandler (SPEC-010).
/// </summary>
public sealed class CreateVehiclePolicyHandlerTests
{
    private readonly IPolicyRepository  _policyRepository;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly ITrmService        _trmService;
    private readonly CreatePolicyHandler _handler;

    public CreateVehiclePolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork       = Substitute.For<IUnitOfWork>();
        _trmService       = Substitute.For<ITrmService>();
        _handler          = new CreatePolicyHandler(_policyRepository, _unitOfWork, _trmService);

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);
    }

    private static CreatePolicyCommand VehicleCommand(
        string planId          = "standard",
        decimal commercialValue = 50_000_000m,
        int vehicleYear        = 2026,
        string brand           = "Toyota") => new()
    {
        IdempotencyKey         = Guid.NewGuid().ToString(),
        Type                   = PolicyType.Vehicle,
        InsuredFirstName       = "Carlos",
        InsuredLastName        = "Pérez",
        InsuredDocumentType    = "CC",
        InsuredDocumentId      = "123456789",
        InsuredBirthDate       = new DateOnly(1990, 1, 1),
        InsuredGender          = "Masculino",
        InsuredAddress         = "Calle 123 # 45-67",
        InsuredCityName        = "Bogotá",
        InsuredPostalCode      = "110111",
        InsuredDepartment      = "Cundinamarca",
        CoverageStartDate      = new DateOnly(2026, 5, 1),
        CoverageEndDate        = new DateOnly(2027, 5, 1),
        VehiclePlanId          = planId,
        VehicleCommercialValue = commercialValue,
        VehicleYear            = vehicleYear,
        VehicleBrand           = brand,
    };

    // ─── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VehiclePolicy_Standard_CreatesPolicy()
    {
        // GIVEN
        var command = VehicleCommand("standard", 50_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Type.Should().Be("Vehicle");
        result.Status.Should().Be("Pending");
        result.VehiclePlan.Should().NotBeNull();
        result.VehiclePlan!.PlanId.Should().Be("standard");
        result.VehiclePlan.PlanName.Should().Be("Plan Estándar");
        result.VehiclePlan.VehicleBrand.Should().Be("Toyota");
        result.VehiclePlan.CommercialValue.Should().Be(50_000_000m);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_Complete_SetsCorrectMonthlyPremium()
    {
        // GIVEN — $50M nuevo Toyota: Plan Completo → mensual = 110,000
        var command = VehicleCommand("complete", 50_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.VehiclePlan!.PlanId.Should().Be("complete");
        result.MonthlyPremium.Should().Be(110_000m);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_InsuredAmountEqualsCommercialValue()
    {
        // GIVEN — insuredAmount debe reflejar el valor comercial del vehículo
        var command = VehicleCommand("premium", 135_000_000m, DateTime.UtcNow.Year, "Toyota");

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.InsuredAmount.Should().Be(135_000_000m);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_RepositoryAddCalledOnce()
    {
        // GIVEN
        var command = VehicleCommand();

        // WHEN
        await _handler.Handle(command, CancellationToken.None);

        // THEN
        await _policyRepository.Received(1).AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    // ─── Brand surcharge ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VehiclePolicy_HighSinistrabilityBrand_HasSurchargeFlag()
    {
        // GIVEN
        var command = VehicleCommand("standard", 30_000_000m, 2023, "Chevrolet");

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.VehiclePlan!.HasBrandSurcharge.Should().BeTrue();
    }

    // ─── Plan inválido (error path) ───────────────────────────────────────────

    [Fact]
    public async Task Handle_VehiclePolicy_InvalidPlanId_ThrowsInvalidVehiclePlanException()
    {
        // GIVEN
        var command = VehicleCommand(planId: "plan-invalido");

        // WHEN
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidVehiclePlanException>();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    // ─── Tipo no-Vehicle no usa VehiclePlan ──────────────────────────────────

    [Fact]
    public async Task Handle_NonVehiclePolicy_VehiclePlanIsNull()
    {
        // GIVEN — tipo Life (no Vehicle), VehiclePlanId presente pero debe ignorarse
        var command = new CreatePolicyCommand
        {
            IdempotencyKey    = Guid.NewGuid().ToString(),
            Type              = PolicyType.Life,
            InsuredFirstName  = "Juan",
            InsuredLastName   = "Vida",
            InsuredDocumentType = "CC",
            InsuredDocumentId = "111222333",
            InsuredBirthDate  = new DateOnly(1985, 1, 1),
            InsuredGender     = "Masculino",
            InsuredAddress    = "Calle 123 # 45-67",
            InsuredCityName   = "Bogotá",
            InsuredPostalCode = "110111",
            InsuredDepartment = "Cundinamarca",
            CoverageStartDate = new DateOnly(2026, 1, 1),
            CoverageEndDate   = new DateOnly(2027, 1, 1),
            MonthlyPremium    = 200m,
            InsuredAmount     = 10_000m,
            VehiclePlanId     = "standard"   // debe ignorarse
        };

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN
        result.Type.Should().Be("Life");
        result.VehiclePlan.Should().BeNull();
    }

    // ─── Idempotencia ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ReturnsExistingPolicyWithoutPersisting()
    {
        // GIVEN — repositorio devuelve la política existente por idempotency key
        var command = VehicleCommand();

        var existingPolicyNumber = PolicyNumber.Create(2026, 42);
        var existingInsured = InsuredPerson.Create("Carlos", "Pérez", "CC", "123456789", new DateOnly(1990, 1, 1), "Masculino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca");
        var existingCoverage = CoveragePeriod.Create(new DateOnly(2026, 5, 1), new DateOnly(2027, 5, 1));
        var existingPolicy = Policy.CreateVehiclePolicy(
            existingPolicyNumber, existingInsured, existingCoverage,
            "standard", 50_000_000m, DateTime.UtcNow.Year, "Toyota",
            DateOnly.FromDateTime(DateTime.UtcNow));

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingPolicy);

        // WHEN
        var result = await _handler.Handle(command, CancellationToken.None);

        // THEN — retorna la existente sin volver a persistir
        result.Should().NotBeNull();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }
}
