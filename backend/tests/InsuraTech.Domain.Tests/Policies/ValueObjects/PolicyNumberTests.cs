namespace InsuraTech.Domain.Tests.Policies.ValueObjects;

using FluentAssertions;
using InsuraTech.Domain.Policies.ValueObjects;

public sealed class PolicyNumberTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnPolicyNumber()
    {
        // Arrange & Act
        var policyNumber = PolicyNumber.Create(2024, 1);

        // Assert
        policyNumber.Value.Should().Be("POL-2024-00000001");
    }

    [Fact]
    public void Create_WithMaxSequence_ShouldFormatCorrectly()
    {
        var policyNumber = PolicyNumber.Create(2024, 99999999);
        policyNumber.Value.Should().Be("POL-2024-99999999");
    }

    [Fact]
    public void Create_WithInvalidYear_ShouldThrowArgumentException()
    {
        // Act
        var act = () => PolicyNumber.Create(1999, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Year is out of valid range*");
    }

    [Fact]
    public void Create_WithZeroSequence_ShouldThrowArgumentException()
    {
        var act = () => PolicyNumber.Create(2024, 0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sequence must be greater than zero*");
    }

    [Fact]
    public void Parse_WithValidFormat_ShouldReturnPolicyNumber()
    {
        var policyNumber = PolicyNumber.Parse("POL-2024-00000001");
        policyNumber.Value.Should().Be("POL-2024-00000001");
    }

    [Fact]
    public void Parse_WithInvalidFormat_ShouldThrowArgumentException()
    {
        var act = () => PolicyNumber.Parse("INVALID-FORMAT");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid policy number format*");
    }

    [Fact]
    public void TwoPolicyNumbers_WithSameValue_ShouldBeEqual()
    {
        var first = PolicyNumber.Parse("POL-2024-00000001");
        var second = PolicyNumber.Parse("POL-2024-00000001");
        first.Should().Be(second);
    }
}