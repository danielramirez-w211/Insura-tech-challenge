namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Commands.ActivatePolicy;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class ActivatePolicyHandlerTests
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ActivatePolicyHandler _handler;

    public ActivatePolicyHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ActivatePolicyHandler(_policyRepository, _unitOfWork);
    }

    private static Policy BuildPendingPolicy() => Policy.Create(
        PolicyNumber.Create(2024, 1),
        PolicyType.Life,
        InsuredPerson.Create("John Doe", "123456789", new DateOnly(1990, 1, 1)),
        CoveragePeriod.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
        100m, 10_000m);

    [Fact]
    public async Task Handle_WithPendingPolicy_ShouldReturnActivePolicy()
    {
        // Arrange
        var policy = BuildPendingPolicy();
        var command = new ActivatePolicyCommand { PolicyId = policy.Id };

        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_WithPendingPolicy_ShouldCallUpdateAsync()
    {
        // Arrange
        var policy = BuildPendingPolicy();
        var command = new ActivatePolicyCommand { PolicyId = policy.Id };

        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _policyRepository.Received(1)
            .UpdateAsync(Arg.Any<Policy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPolicy_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ActivatePolicyCommand { PolicyId = Guid.NewGuid() };

        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*was not found*");
    }
}