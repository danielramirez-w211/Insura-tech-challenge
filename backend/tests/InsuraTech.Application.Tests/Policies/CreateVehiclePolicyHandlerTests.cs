namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class CreateVehiclePolicyHandlerTests
{
    private readonly IPolicyRepository   _policyRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly CreatePolicyHandler _handler;

    public CreateVehiclePolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork       = Substitute.For<IUnitOfWork>();

        var trmService = Substitute.For<ITrmService>();
        var strategies = new ICreatePolicyStrategy[]
        {
            new CreateHealthPolicyStrategy(),
            new CreateLifePolicyStrategy(),
            new CreateVehiclePolicyStrategy(),
            new CreateHomePolicyStrategy(),
            new CreateTravelPolicyStrategy(trmService),
        };

        _handler = new CreatePolicyHandler(_policyRepository, _unitOfWork, strategies);

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);
    }

    private static CreatePolicyCommand VehicleCommand(
        string planId           = "standard",
        decimal commercialValue = 50_000_000m,
        int vehicleYear         = 2026,
        string brand            = "Toyota") => new()
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

    [Fact]
    public async Task Handle_VehiclePolicy_Standard_CreatesPolicy()
    {
        var result = await _handler.Handle(VehicleCommand("standard", 50_000_000m, DateTime.UtcNow.Year, "Toyota"), CancellationToken.None);

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
        var result = await _handler.Handle(VehicleCommand("complete", 50_000_000m, DateTime.UtcNow.Year, "Toyota"), CancellationToken.None);

        result.VehiclePlan!.PlanId.Should().Be("complete");
        result.MonthlyPremium.Should().Be(110_000m);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_InsuredAmountEqualsCommercialValue()
    {
        var result = await _handler.Handle(VehicleCommand("premium", 135_000_000m, DateTime.UtcNow.Year, "Toyota"), CancellationToken.None);

        result.InsuredAmount.Should().Be(135_000_000m);
    }

    [Fact]
    public async Task Handle_VehiclePolicy_RepositoryAddCalledOnce()
    {
        await _handler.Handle(VehicleCommand(), CancellationToken.None);

        await _policyRepository.Received(1).AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_VehiclePolicy_HighSinistrabilityBrand_HasSurchargeFlag()
    {
        var result = await _handler.Handle(VehicleCommand("standard", 30_000_000m, 2023, "Chevrolet"), CancellationToken.None);

        result.VehiclePlan!.HasBrandSurcharge.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_VehiclePolicy_InvalidPlanId_ThrowsInvalidVehiclePlanException()
    {
        var act = async () => await _handler.Handle(VehicleCommand(planId: "plan-invalido"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidVehiclePlanException>();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonVehiclePolicy_VehiclePlanIsNull()
    {
        var command = new CreatePolicyCommand
        {
            IdempotencyKey      = Guid.NewGuid().ToString(),
            Type                = PolicyType.Life,
            InsuredFirstName    = "Juan",
            InsuredLastName     = "Vida",
            InsuredDocumentType = "CC",
            InsuredDocumentId   = "111222333",
            InsuredBirthDate    = new DateOnly(1985, 1, 1),
            InsuredGender       = "Masculino",
            InsuredAddress      = "Calle 123 # 45-67",
            InsuredCityName     = "Bogotá",
            InsuredPostalCode   = "110111",
            InsuredDepartment   = "Cundinamarca",
            CoverageStartDate   = new DateOnly(2026, 1, 1),
            CoverageEndDate     = new DateOnly(2027, 1, 1),
            MonthlyPremium      = 200m,
            InsuredAmount       = 10_000m,
            VehiclePlanId       = "standard"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Type.Should().Be("Life");
        result.VehiclePlan.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ReturnsExistingPolicyWithoutPersisting()
    {
        var command         = VehicleCommand();
        var existingPolicy  = Policy.CreateVehiclePolicy(
            PolicyNumber.Create(2026, 42),
            InsuredPerson.Create("Carlos", "Pérez", "CC", "123456789", new DateOnly(1990, 1, 1), "Masculino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
            CoveragePeriod.Create(new DateOnly(2026, 5, 1), new DateOnly(2027, 5, 1)),
            "standard", 50_000_000m, DateTime.UtcNow.Year, "Toyota",
            DateOnly.FromDateTime(DateTime.UtcNow));

        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingPolicy);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }
}
