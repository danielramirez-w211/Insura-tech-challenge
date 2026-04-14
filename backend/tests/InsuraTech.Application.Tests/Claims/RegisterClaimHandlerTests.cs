namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Commands.RegisterClaim;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class RegisterClaimHandlerTests
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterClaimHandler _handler;

    public RegisterClaimHandlerTests()
    {
        _claimRepository = Substitute.For<IClaimRepository>();
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RegisterClaimHandler(_claimRepository, _policyRepository, _unitOfWork);
    }

    private static Policy BuildActivePolicy()
    {
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("John", "Doe", "CC", "123456789", new DateOnly(1990, 1, 1), "Masculino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        policy.Activate();
        return policy;
    }

    private static RegisterClaimCommand BuildCommand(Guid policyId) => new()
    {
        PolicyId = policyId,
        Type = ClaimType.Accident,
        ClaimedAmount = 1_000m,
        IncidentDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Description = "Accident on highway",
        ResponsibleUser = "agent@insuratech.com"
    };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterClaim()
    {
        // Arrange
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);
        _claimRepository.CountOpenClaimsByPolicyIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(BuildCommand(policy.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Registered");
        result.ClaimedAmount.Should().Be(1_000m);
    }

    [Fact]
    public async Task Handle_WithNonExistentPolicy_ShouldThrowNotFoundException()
    {
        // Arrange
        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);

        // Act
        var act = async () => await _handler.Handle(
            BuildCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInactivePolicy_ShouldThrowBusinessRuleException()
    {
        // Arrange — Policy en estado Pending (no activa)
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            InsuraTech.Domain.Policies.PolicyType.Life,
            InsuredPerson.Create("John", "Doe", "CC", "123456789", new DateOnly(1990, 1, 1), "Masculino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);

        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        // Act
        var act = async () => await _handler.Handle(
            BuildCommand(policy.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallAddAndSave()
    {
        // Arrange
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);
        _claimRepository.CountOpenClaimsByPolicyIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        await _handler.Handle(BuildCommand(policy.Id), CancellationToken.None);

        // Assert
        await _claimRepository.Received(1)
            .AddAsync(Arg.Any<InsuraTech.Domain.Claims.Claim>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}