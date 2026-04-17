namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CreatePolicy;
using InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class CreatePolicyHandlerTests
{
    private readonly IPolicyRepository   _policyRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly CreatePolicyHandler _handler;

    public CreatePolicyHandlerTests()
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
    }

    private static CreatePolicyCommand BuildCommand(string idempotencyKey = "key-001") =>
        new()
        {
            IdempotencyKey      = idempotencyKey,
            Type                = PolicyType.Life,
            InsuredFirstName    = "John",
            InsuredLastName     = "Doe",
            InsuredDocumentType = "CC",
            InsuredDocumentId   = "123456789",
            InsuredBirthDate    = new DateOnly(1990, 1, 1),
            InsuredGender       = "Masculino",
            InsuredAddress      = "Calle 123 # 45-67",
            InsuredCityName     = "Bogotá",
            InsuredPostalCode   = "110111",
            InsuredDepartment   = "Cundinamarca",
            CoverageStartDate   = DateOnly.FromDateTime(DateTime.UtcNow),
            CoverageEndDate     = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1),
            MonthlyPremium      = 100m,
            InsuredAmount       = 10_000m
        };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePolicy()
    {
        // Arrange
        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);

        // Act
        var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        result.InsuredFirstName.Should().Be("John");
        result.InsuredLastName.Should().Be("Doe");
        result.PolicyNumber.Should().StartWith("POL-");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallAddAsync()
    {
        // Arrange
        _policyRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>())
            .Returns(1L);

        // Act
        await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        await _policyRepository.Received(1)
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingIdempotencyKey_ShouldReturnExistingPolicy()
    {
        // Arrange
        var existingPolicy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("John", "Doe", "CC", "123456789", new DateOnly(1990, 1, 1), "Masculino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);

        _policyRepository.GetByIdempotencyKeyAsync("key-001", Arg.Any<CancellationToken>())
            .Returns(existingPolicy);

        // Act
        var result = await _handler.Handle(BuildCommand("key-001"), CancellationToken.None);

        // Assert
        result.PolicyNumber.Should().Be("POL-2024-00000001");
        await _policyRepository.DidNotReceive()
            .AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
    }
}
