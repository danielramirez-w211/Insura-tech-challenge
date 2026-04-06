namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Commands.RejectClaim;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public sealed class RejectClaimHandlerTests
{
    private readonly IClaimRepository _claimRepository = Substitute.For<IClaimRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private RejectClaimHandler Sut => new(_claimRepository, _unitOfWork);

    private static Policy BuildActivePolicy()
    {
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("Ana Lopez", "12345678", new DateOnly(1990, 1, 1)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        policy.Activate();
        return policy;
    }

    private static DomainClaim BuildClaimUnderInvestigation()
    {
        var policy = BuildActivePolicy();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var claim = DomainClaim.Register(
            policy.Id, ClaimType.Accident, 1_000m, today,
            "Accident description",
            today.AddDays(-30), today.AddYears(1),
            10_000m, 0, "agent@test.com");
        claim.StartInvestigation("investigator@test.com");
        return claim;
    }

    [Fact]
    public async Task Handle_WhenClaimIsUnderInvestigation_ShouldRejectAndReturnResponse()
    {
        // GIVEN
        var claim = BuildClaimUnderInvestigation();
        _claimRepository.GetByIdAsync(claim.Id).Returns(claim);

        // WHEN
        var result = await Sut.Handle(
            new RejectClaimCommand { ClaimId = claim.Id, Reason = "Not covered", ResponsibleUser = "agent@test.com" },
            CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(nameof(ClaimStatus.Rejected));
        result.RejectionReason.Should().Be("Not covered");
    }

    [Fact]
    public async Task Handle_WhenClaimNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _claimRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((DomainClaim?)null);

        // WHEN
        var act = async () => await Sut.Handle(
            new RejectClaimCommand { ClaimId = Guid.NewGuid(), Reason = "Not covered", ResponsibleUser = "agent@test.com" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenClaimNotInInvestigation_ShouldThrowInvalidStateException()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var claim = DomainClaim.Register(
            policy.Id, ClaimType.Accident, 1_000m, today,
            "Accident description",
            today.AddDays(-30), today.AddYears(1),
            10_000m, 0, "agent@test.com");
        _claimRepository.GetByIdAsync(claim.Id).Returns(claim);

        // WHEN
        var act = async () => await Sut.Handle(
            new RejectClaimCommand { ClaimId = claim.Id, Reason = "Not covered", ResponsibleUser = "agent@test.com" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidClaimStateException>();
    }
}
