namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.RenewPolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class RenewPolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private RenewPolicyHandler Sut => new(_policyRepository, _unitOfWork);

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
    public async Task Handle_WhenPolicyIsActive_ShouldRenewAndReturnResponse()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>()).Returns(2L);

        var newStart = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        var newEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));

        // WHEN
        var result = await Sut.Handle(
            new RenewPolicyCommand { PolicyId = policy.Id, NewCoverageStartDate = newStart, NewCoverageEndDate = newEnd },
            CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(nameof(PolicyStatus.Pending));
    }

    [Fact]
    public async Task Handle_WhenPolicyIsActive_ShouldCallAddAndSave()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>()).Returns(2L);

        var newStart = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        var newEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));

        // WHEN
        await Sut.Handle(
            new RenewPolicyCommand { PolicyId = policy.Id, NewCoverageStartDate = newStart, NewCoverageEndDate = newEnd },
            CancellationToken.None);

        // THEN
        await _policyRepository.Received(1).AddAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPolicyNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Policy?)null);

        // WHEN
        var act = async () => await Sut.Handle(
            new RenewPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                NewCoverageStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                NewCoverageEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2))
            },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPolicyIsPending_ShouldThrowInvalidStateException()
    {
        // GIVEN
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 3),
            PolicyType.Life,
            InsuredPerson.Create("Ana", "Lopez", "CC", "12345678", new DateOnly(1990, 1, 1)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);
        _policyRepository.GetNextSequenceAsync(Arg.Any<CancellationToken>()).Returns(4L);

        // WHEN
        var act = async () => await Sut.Handle(
            new RenewPolicyCommand
            {
                PolicyId = policy.Id,
                NewCoverageStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                NewCoverageEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2))
            },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidPolicyStateException>();
    }
}
