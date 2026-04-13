namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Queries.GetPolicies;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class GetPoliciesHandlerTests
{
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private GetPoliciesHandler Sut => new(_policyRepository);

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

    [Fact]
    public async Task Handle_WhenPoliciesExist_ShouldReturnPagedResult()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetAllAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new[] { policy });
        _policyRepository.CountAsync(null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(1);

        // WHEN
        var result = await Sut.Handle(new GetPoliciesQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenNoPoliciesExist_ShouldReturnEmptyPagedResult()
    {
        // GIVEN
        _policyRepository.GetAllAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Policy>());
        _policyRepository.CountAsync(null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(0);

        // WHEN
        var result = await Sut.Handle(new GetPoliciesQuery(), CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetAllAndCount()
    {
        // GIVEN
        _policyRepository.GetAllAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Policy>());
        _policyRepository.CountAsync(null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(0);

        // WHEN
        await Sut.Handle(new GetPoliciesQuery(), CancellationToken.None);

        // THEN
        await _policyRepository.Received(1)
            .GetAllAsync(null, null, null, null, null, 1, 10, Arg.Any<CancellationToken>());
        await _policyRepository.Received(1)
            .CountAsync(null, null, null, null, null, Arg.Any<CancellationToken>());
    }
}
