namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Commands.ApproveClaim;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public sealed class ApproveClaimHandlerTests
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApproveClaimHandler _handler;

    public ApproveClaimHandlerTests()
    {
        _claimRepository = Substitute.For<IClaimRepository>();
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ApproveClaimHandler(_claimRepository, _policyRepository, _unitOfWork);
    }

    private static Policy BuildActivePolicy()
    {
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("John Doe", "123456789", new DateOnly(1990, 1, 1)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        policy.Activate();
        return policy;
    }

    private static DomainClaim BuildClaimUnderInvestigation(Guid policyId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var claim = DomainClaim.Register(
            policyId, ClaimType.Accident, 1_000m, today,
            "Accident description",
            today.AddDays(-30), today.AddYears(1),
            10_000m, 0, "agent@insuratech.com");
        claim.StartInvestigation("investigator@insuratech.com");
        return claim;
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldApproveClaimAndDeductAmount()
    {
        // Arrange
        var policy = BuildActivePolicy();
        var claim = BuildClaimUnderInvestigation(policy.Id);

        _claimRepository.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>())
            .Returns(claim);
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        var command = new ApproveClaimCommand
        {
            ClaimId = claim.Id,
            ApprovedAmount = 800m,
            ResponsibleUser = "approver@insuratech.com"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Approved");
        result.ApprovedAmount.Should().Be(800m);
        policy.AvailableInsuredAmount.Should().Be(9_200m);
    }

    [Fact]
    public async Task Handle_WithNonExistentClaim_ShouldThrowNotFoundException()
    {
        // Arrange
        _claimRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((DomainClaim?)null);

        var command = new ApproveClaimCommand
        {
            ClaimId = Guid.NewGuid(),
            ApprovedAmount = 800m,
            ResponsibleUser = "approver@insuratech.com"
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenApproved_ShouldUpdateBothClaimAndPolicy()
    {
        // Arrange
        var policy = BuildActivePolicy();
        var claim = BuildClaimUnderInvestigation(policy.Id);

        _claimRepository.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>())
            .Returns(claim);
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        var command = new ApproveClaimCommand
        {
            ClaimId = claim.Id,
            ApprovedAmount = 800m,
            ResponsibleUser = "approver@insuratech.com"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — ambos deben actualizarse
        await _claimRepository.Received(1)
            .UpdateAsync(Arg.Any<DomainClaim>(), Arg.Any<CancellationToken>());
        await _policyRepository.Received(1)
            .UpdateAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}