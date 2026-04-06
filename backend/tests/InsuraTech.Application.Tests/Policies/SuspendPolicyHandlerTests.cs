namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.SuspendPolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class SuspendPolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private SuspendPolicyHandler Sut => new(_policyRepository, _unitOfWork);

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

    [Fact]
    public async Task Handle_WhenPolicyIsActive_ShouldSuspendAndReturnResponse()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        var result = await Sut.Handle(
            new SuspendPolicyCommand { PolicyId = policy.Id, Reason = "Non-payment" },
            CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(nameof(PolicyStatus.Suspended));
    }

    [Fact]
    public async Task Handle_WhenPolicyIsActive_ShouldCallUpdateAndSave()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        await Sut.Handle(
            new SuspendPolicyCommand { PolicyId = policy.Id, Reason = "Non-payment" },
            CancellationToken.None);

        // THEN
        await _policyRepository.Received(1).UpdateAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPolicyNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Policy?)null);

        // WHEN
        var act = async () => await Sut.Handle(
            new SuspendPolicyCommand { PolicyId = Guid.NewGuid(), Reason = "Non-payment" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPolicyNotActive_ShouldThrowInvalidStateException()
    {
        // GIVEN
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 2),
            PolicyType.Life,
            InsuredPerson.Create("Ana Lopez", "12345678", new DateOnly(1990, 1, 1)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            100m, 10_000m);
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        var act = async () => await Sut.Handle(
            new SuspendPolicyCommand { PolicyId = policy.Id, Reason = "Non-payment" },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<InvalidPolicyStateException>();
    }
}
