namespace InsuraTech.Domain.Tests.Policies;

using FluentAssertions;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;

public sealed class PolicyTests
{
    // ─── Factory ───────────────────────────────────────────────────────────────
    private static Policy CreateValidPolicy(
        decimal insuredAmount = 10_000m,
        decimal monthlyPremium = 100m)
    {
        var number = PolicyNumber.Create(2024, 1);
        var insured = InsuredPerson.Create(
            "John Doe",
            "123456789",
            new DateOnly(1990, 1, 1));
        var coverage = CoveragePeriod.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1));

        return Policy.Create(number, PolicyType.Life, insured, coverage,
            monthlyPremium, insuredAmount);
    }

    // ─── Create ────────────────────────────────────────────────────────────────
    [Fact]
    public void Create_WithValidData_ShouldHavePendingStatus()
    {
        var policy = CreateValidPolicy();
        policy.Status.Should().Be(PolicyStatus.Pending);
    }

    [Fact]
    public void Create_WithValidData_ShouldSetAvailableAmountEqualToInsuredAmount()
    {
        var policy = CreateValidPolicy(insuredAmount: 10_000m);
        policy.AvailableInsuredAmount.Should().Be(10_000m);
    }

    [Fact]
    public void Create_WithPremiumExceeding5Percent_ShouldThrowArgumentException()
    {
        var act = () => CreateValidPolicy(insuredAmount: 10_000m, monthlyPremium: 600m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 5%*");
    }

    [Fact]
    public void Create_WithZeroInsuredAmount_ShouldThrowArgumentException()
    {
        var act = () => CreateValidPolicy(insuredAmount: 0m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Insured amount must be greater than zero*");
    }

    // ─── Activate ──────────────────────────────────────────────────────────────
    [Fact]
    public void Activate_WhenPending_ShouldChangeStatusToActive()
    {
        var policy = CreateValidPolicy();
        policy.Activate();
        policy.Status.Should().Be(PolicyStatus.Active);
    }

    [Fact]
    public void Activate_WhenPending_ShouldEmitPolicyActivatedEvent()
    {
        var policy = CreateValidPolicy();
        policy.Activate();

        policy.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PolicyActivatedEvent>();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowInvalidPolicyStateException()
    {
        var policy = CreateValidPolicy();
        policy.Activate();

        var act = () => policy.Activate();

        act.Should().Throw<InvalidPolicyStateException>()
            .WithMessage("*Active*");
    }

    // ─── Suspend ───────────────────────────────────────────────────────────────
    [Fact]
    public void Suspend_WhenActive_ShouldChangeStatusToSuspended()
    {
        var policy = CreateValidPolicy();
        policy.Activate();
        policy.Suspend("Non-payment");

        policy.Status.Should().Be(PolicyStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenPending_ShouldThrowInvalidPolicyStateException()
    {
        var policy = CreateValidPolicy();
        var act = () => policy.Suspend("reason");

        act.Should().Throw<InvalidPolicyStateException>();
    }

    // ─── Cancel ────────────────────────────────────────────────────────────────
    [Fact]
    public void Cancel_WhenActive_ShouldChangeStatusToCancelled()
    {
        var policy = CreateValidPolicy();
        policy.Activate();
        policy.Cancel("Client request", DateOnly.FromDateTime(DateTime.UtcNow));

        policy.Status.Should().Be(PolicyStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidPolicyStateException()
    {
        var policy = CreateValidPolicy();
        policy.Activate();
        policy.Cancel("First reason", DateOnly.FromDateTime(DateTime.UtcNow));

        var act = () => policy.Cancel("Second reason", DateOnly.FromDateTime(DateTime.UtcNow));

        act.Should().Throw<InvalidPolicyStateException>()
            .WithMessage("*Cancelled*");
    }

    // ─── DeductInsuredAmount ───────────────────────────────────────────────────
    [Fact]
    public void DeductInsuredAmount_WithValidAmount_ShouldReduceAvailableAmount()
    {
        var policy = CreateValidPolicy(insuredAmount: 10_000m);
        policy.Activate();
        policy.DeductInsuredAmount(3_000m);

        policy.AvailableInsuredAmount.Should().Be(7_000m);
    }

    [Fact]
    public void DeductInsuredAmount_WhenAmountExceedsAvailable_ShouldThrowBusinessRuleException()
    {
        var policy = CreateValidPolicy(insuredAmount: 10_000m);
        policy.Activate();

        var act = () => policy.DeductInsuredAmount(15_000m);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*exceeds available insured amount*");
    }

    [Fact]
    public void DeductInsuredAmount_WhenAmountEqualsAvailable_ShouldSetStatusToExhausted()
    {
        var policy = CreateValidPolicy(insuredAmount: 10_000m);
        policy.Activate();
        policy.DeductInsuredAmount(10_000m);

        policy.Status.Should().Be(PolicyStatus.Exhausted);
        policy.AvailableInsuredAmount.Should().Be(0m);
    }

    // ─── StatusHistory ─────────────────────────────────────────────────────────
    [Fact]
    public void Create_ShouldAddPendingStatusToHistory()
    {
        var policy = CreateValidPolicy();

        policy.StatusHistory.Should().ContainSingle()
            .Which.Status.Should().Be(PolicyStatus.Pending);
    }

    [Fact]
    public void Activate_ShouldAddActiveStatusToHistory()
    {
        var policy = CreateValidPolicy();
        policy.Activate();

        policy.StatusHistory.Should().HaveCount(2);
        policy.StatusHistory.Last().Status.Should().Be(PolicyStatus.Active);
    }
}