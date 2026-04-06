namespace InsuraTech.Application.Tests.Policies;

using FluentAssertions;
using InsuraTech.Application.Policies.Queries.GetPolicyById;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using NSubstitute;

public sealed class GetPolicyByIdHandlerTests
{
    private readonly IPolicyRepository _policyRepository;
    private readonly GetPolicyByIdHandler _handler;

    public GetPolicyByIdHandlerTests()
    {
        _policyRepository = Substitute.For<IPolicyRepository>();
        _handler = new GetPolicyByIdHandler(_policyRepository);
    }

    [Fact]
    public async Task Handle_WithExistingPolicy_ShouldReturnPolicyResponse()
    {
        // Arrange
        var policy = Policy.Create(
            PolicyNumber.Create(2024, 1),
            PolicyType.Vehicle,
            InsuredPerson.Create("Jane Doe", "987654321", new DateOnly(1985, 6, 15)),
            CoveragePeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)),
            150m, 15_000m);

        _policyRepository.GetByIdAsync(policy.Id, Arg.Any<CancellationToken>())
            .Returns(policy);

        var query = new GetPolicyByIdQuery { PolicyId = policy.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(policy.Id);
        result.Type.Should().Be("Vehicle");
        result.InsuredDocumentId.Should().Be("987654321");
    }

    [Fact]
    public async Task Handle_WithNonExistentPolicy_ShouldThrowNotFoundException()
    {
        // Arrange
        _policyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Policy?)null);

        var query = new GetPolicyByIdQuery { PolicyId = Guid.NewGuid() };

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}