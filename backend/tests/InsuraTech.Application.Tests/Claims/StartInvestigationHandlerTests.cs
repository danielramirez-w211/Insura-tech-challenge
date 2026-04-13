namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Commands.StartInvestigation;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public sealed class StartInvestigationHandlerTests
{
    private readonly IClaimRepository _claimRepository = Substitute.For<IClaimRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private StartInvestigationHandler Sut => new(_claimRepository, _unitOfWork);

    private static Policy BuildActivePolicy()
    {
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("Ana", "Lopez", "CC", "12345678", new DateOnly(1990, 1, 1), "Femenino", "Calle 123 # 45-67", "Bogotá", "110111", "Cundinamarca"),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        policy.Activate();
        return policy;
    }

    private static DomainClaim BuildRegisteredClaim()
    {
        var policy = BuildActivePolicy();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return DomainClaim.Register(
            policy.Id, ClaimType.Accident, 1_000m, today,
            "Accident description",
            today.AddDays(-30), today.AddYears(1),
            10_000m, 0, "agent@test.com");
    }

    [Fact]
    public async Task Handle_WhenClaimIsRegistered_ShouldStartInvestigationAndReturnResponse()
    {
        // GIVEN
        var claim = BuildRegisteredClaim();
        _claimRepository.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        // WHEN
        var result = await Sut.Handle(
            new StartInvestigationCommand { ClaimId = claim.Id, ResponsibleUser = "investigator@test.com" },
            CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(nameof(ClaimStatus.UnderInvestigation));
    }

    [Fact]
    public async Task Handle_WhenClaimIsRegistered_ShouldCallUpdateAndSave()
    {
        // GIVEN
        var claim = BuildRegisteredClaim();
        _claimRepository.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        // WHEN
        await Sut.Handle(
            new StartInvestigationCommand { ClaimId = claim.Id, ResponsibleUser = "investigator@test.com" },
            CancellationToken.None);

        // THEN
        await _claimRepository.Received(1).UpdateAsync(Arg.Any<DomainClaim>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClaimNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _claimRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DomainClaim?)null);

        // WHEN
        var act = async () => await Sut.Handle(
            new StartInvestigationCommand { ClaimId = Guid.NewGuid(), ResponsibleUser = "investigator@test.com" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenClaimAlreadyUnderInvestigation_ShouldThrowInvalidStateException()
    {
        // GIVEN
        var claim = BuildRegisteredClaim();
        claim.StartInvestigation("investigator@test.com");
        _claimRepository.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>()).Returns(claim);

        // WHEN
        var act = async () => await Sut.Handle(
            new StartInvestigationCommand { ClaimId = claim.Id, ResponsibleUser = "investigator@test.com" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidClaimStateException>();
    }
}
