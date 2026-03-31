namespace InsuraTech.Domain.Tests.Claims;

using FluentAssertions;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public sealed class ClaimTests
{
    private static DomainClaim CreateValidClaim(
        int openClaimsCount = 0,
        decimal claimedAmount = 1_000m,
        decimal availableAmount = 10_000m)
    {
        var policyId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return DomainClaim.Register(
            policyId,
            ClaimType.Accident,
            claimedAmount,
            today,
            "Accident description",
            today.AddDays(-30),
            today.AddYears(1),
            availableAmount,
            openClaimsCount,
            "agent@insuratech.com");
    }

    // ─── Register ──────────────────────────────────────────────────────────────
    [Fact]
    public void Register_WithValidData_ShouldHaveRegisteredStatus()
    {
        var claim = CreateValidClaim();
        claim.Status.Should().Be(ClaimStatus.Registered);
    }

    [Fact]
    public void Register_With3OpenClaims_ShouldThrowClaimLimitExceededException()
    {
        var act = () => CreateValidClaim(openClaimsCount: 3);
        act.Should().Throw<ClaimLimitExceededException>()
            .WithMessage("*already has 3 open claims*");
    }

    [Fact]
    public void Register_WithAmountExceedingAvailable_ShouldThrowBusinessRuleException()
    {
        var act = () => CreateValidClaim(claimedAmount: 15_000m, availableAmount: 10_000m);
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*exceeds available insured amount*");
    }

    [Fact]
    public void Register_WithIncidentDateOutsideCoverage_ShouldThrowClaimOnExpiredPolicyException()
    {
        var policyId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var act = () => DomainClaim.Register(
            policyId,
            ClaimType.Accident,
            1_000m,
            today.AddYears(-2),  // Fecha fuera de vigencia
            "Description",
            today.AddDays(-30),
            today.AddYears(1),
            10_000m,
            0,
            "agent@insuratech.com");

        act.Should().Throw<ClaimOnExpiredPolicyException>()
            .WithMessage("*outside the coverage period*");
    }

    // ─── StartInvestigation ────────────────────────────────────────────────────
    [Fact]
    public void StartInvestigation_WhenRegistered_ShouldChangeStatusToUnderInvestigation()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");

        claim.Status.Should().Be(ClaimStatus.UnderInvestigation);
    }

    [Fact]
    public void StartInvestigation_WhenNotRegistered_ShouldThrowInvalidClaimStateException()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");

        var act = () => claim.StartInvestigation("investigator@insuratech.com");

        act.Should().Throw<InvalidClaimStateException>();
    }

    // ─── Approve ───────────────────────────────────────────────────────────────
    [Fact]
    public void Approve_WhenUnderInvestigation_ShouldChangeStatusToApproved()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");
        claim.Approve(800m, "approver@insuratech.com");

        claim.Status.Should().Be(ClaimStatus.Approved);
        claim.ApprovedAmount.Should().Be(800m);
    }

    [Fact]
    public void Approve_WithAmountGreaterThanClaimed_ShouldThrowArgumentException()
    {
        var claim = CreateValidClaim(claimedAmount: 1_000m);
        claim.StartInvestigation("investigator@insuratech.com");

        var act = () => claim.Approve(2_000m, "approver@insuratech.com");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*claimed amount*");
    }

    // ─── Reject ────────────────────────────────────────────────────────────────
    [Fact]
    public void Reject_WhenUnderInvestigation_ShouldChangeStatusToRejected()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");
        claim.Reject("Insufficient evidence", "reviewer@insuratech.com");

        claim.Status.Should().Be(ClaimStatus.Rejected);
        claim.RejectionReason.Should().Be("Insufficient evidence");
    }

    // ─── Appeal ────────────────────────────────────────────────────────────────
    [Fact]
    public void Appeal_WhenRejected_ShouldChangeStatusToAppealed()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");
        claim.Reject("Insufficient evidence", "reviewer@insuratech.com");
        claim.Appeal("agent@insuratech.com", "New evidence provided");

        claim.Status.Should().Be(ClaimStatus.Appealed);
        claim.HasBeenAppealed.Should().BeTrue();
    }

    [Fact]
    public void Appeal_WhenAlreadyAppealed_ShouldThrowBusinessRuleException()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");
        claim.Reject("Insufficient evidence", "reviewer@insuratech.com");
        claim.Appeal("agent@insuratech.com");
        claim.Reject("Still insufficient", "reviewer@insuratech.com");

        var act = () => claim.Appeal("agent@insuratech.com");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*already been appealed*");
    }

    // ─── RegisterPayment ───────────────────────────────────────────────────────
    [Fact]
    public void RegisterPayment_WhenApproved_ShouldChangeStatusToPaid()
    {
        var claim = CreateValidClaim();
        claim.StartInvestigation("investigator@insuratech.com");
        claim.Approve(800m, "approver@insuratech.com");
        claim.RegisterPayment("finance@insuratech.com");

        claim.Status.Should().Be(ClaimStatus.Paid);
    }

    [Fact]
    public void RegisterPayment_WhenNotApproved_ShouldThrowInvalidClaimStateException()
    {
        var claim = CreateValidClaim();
        var act = () => claim.RegisterPayment("finance@insuratech.com");

        act.Should().Throw<InvalidClaimStateException>();
    }
}