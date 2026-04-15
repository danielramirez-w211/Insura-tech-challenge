namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.CancelPolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class CancelPolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository = Substitute.For<IPolicyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private CancelPolicyHandler Sut => new(_policyRepository, _unitOfWork);

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

    [Fact]
    public async Task Handle_WhenPolicyIsActive_ShouldCancelAndReturnResponse()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        var effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        var result = await Sut.Handle(
            new CancelPolicyCommand { PolicyId = policy.Id, Reason = "Customer request", EffectiveDate = effectiveDate },
            CancellationToken.None);

        // THEN
        result.Should().NotBeNull();
        result.Status.Should().Be(nameof(PolicyStatus.Cancelled));
    }

    [Fact]
    public async Task Handle_WhenPolicyIsActive_ShouldCallUpdateAndSave()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        var effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        await Sut.Handle(
            new CancelPolicyCommand { PolicyId = policy.Id, Reason = "Customer request", EffectiveDate = effectiveDate },
            CancellationToken.None);

        // THEN
        await _policyRepository.Received(1).UpdateAsync(Arg.Any<Policy>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPolicyNotFound_ShouldThrowNotFoundException()
    {
        // GIVEN
        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Policy?)null);

        // WHEN
        var act = async () => await Sut.Handle(
            new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "Customer request",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenEffectiveDateIsInPast_ShouldThrowArgumentException()
    {
        // GIVEN
        var policy = BuildActivePolicy();
        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>()).Returns(policy);

        // WHEN
        var act = async () => await Sut.Handle(
            new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5))
            },
            CancellationToken.None);

        // THEN
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
