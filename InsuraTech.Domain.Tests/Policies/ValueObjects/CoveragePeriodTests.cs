namespace InsuraTech.Domain.Tests.Policies.ValueObjects;

using FluentAssertions;
using InsuraTech.Domain.Policies.ValueObjects;

public sealed class CoveragePeriodTests
{
    [Fact]
    public void Create_WithValidDates_ShouldReturnCoveragePeriod()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddMonths(6);

        var coverage = CoveragePeriod.Create(start, end);

        coverage.StartDate.Should().Be(start);
        coverage.EndDate.Should().Be(end);
    }

    [Fact]
    public void Create_WithStartAfterEnd_ShouldThrowArgumentException()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.UtcNow);

        var act = () => CoveragePeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Start date must be before end date*");
    }

    [Fact]
    public void Create_WithLessThan30Days_ShouldThrowArgumentException()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(10);

        var act = () => CoveragePeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least 30 days*");
    }

    [Fact]
    public void Create_WithMoreThan5Years_ShouldThrowArgumentException()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddYears(6);

        var act = () => CoveragePeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 5 years*");
    }

    [Fact]
    public void IsActive_WithDateInRange_ShouldReturnTrue()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var end = start.AddMonths(6);
        var coverage = CoveragePeriod.Create(start, end);

        coverage.IsActive(DateOnly.FromDateTime(DateTime.UtcNow)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithDateOutOfRange_ShouldReturnFalse()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var coverage = CoveragePeriod.Create(start, end);

        coverage.IsActive(DateOnly.FromDateTime(DateTime.UtcNow)).Should().BeFalse();
    }
}