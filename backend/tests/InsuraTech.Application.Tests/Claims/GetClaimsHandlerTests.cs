namespace InsuraTech.Application.Tests.Claims;

using FluentAssertions;
using InsuraTech.Application.Claims.Queries.GetClaims;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public sealed class GetClaimsHandlerTests
{
    private readonly IClaimRepository _claimRepository = Substitute.For<IClaimRepository>();
    private GetClaimsHandler Sut => new(_claimRepository);

    private static Policy BuildActivePolicy()
    {
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Life,
            InsuredPerson.Create("Ana", "Lopez", "CC", "12345678", new DateOnly(1990, 1, 1)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        policy.Activate();
        return policy;
    }

    private static DomainClaim BuildRegisteredClaim(Guid policyId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return DomainClaim.Register(
            policyId, ClaimType.Accident, 1_000m, today,
            "Accident description",
            today.AddDays(-30), today.AddYears(1),
            10_000m, 0, "agent@test.com");
    }

    [Fact]
    public async Task Handle_WhenClaimsExist_ShouldReturnPagedResult()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        var claim = BuildRegisteredClaim(policy.Id);
        _claimRepository.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(1);
        _claimRepository.GetAllAsync(null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new[] { claim });

        // WHEN
        var result = await Sut.Handle(new GetClaimsQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenNoClaimsExist_ShouldReturnEmptyPagedResult()
    {
        // GIVEN
        _claimRepository.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(0);

        // WHEN
        var result = await Sut.Handle(new GetClaimsQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenClaimsExist_ShouldCallCountAndGetAll()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        var claim = BuildRegisteredClaim(policy.Id);
        _claimRepository.CountAsync(null, null, Arg.Any<CancellationToken>()).Returns(1);
        _claimRepository.GetAllAsync(null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new[] { claim });

        // WHEN
        await Sut.Handle(new GetClaimsQuery(), CancellationToken.None);

        // THEN
        await _claimRepository.Received(1).CountAsync(null, null, Arg.Any<CancellationToken>());
        await _claimRepository.Received(1).GetAllAsync(null, null, 1, 10, Arg.Any<CancellationToken>());
    }
}
