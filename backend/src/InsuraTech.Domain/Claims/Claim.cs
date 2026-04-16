using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Exceptions;

namespace InsuraTech.Domain.Claims
{
    public sealed class Claim : AggregateRoot
    {
        public const int MaxOpenClaimsPerPolicy = 3;

        public Guid PolicyId { get; private set; }
        public ClaimType Type { get; private set; }
        public ClaimStatus Status { get; private set; }
        public decimal ClaimedAmount { get; private set; }
        public decimal? ApprovedAmount { get; private set; }
        public DateOnly IncidentDate { get; private set; }
        public string Description { get; private set; } = null!;
        public bool HasBeenAppealed { get; private set; }
        public string? RejectionReason { get; private set; }
        public Guid? CreatedByAdvisorId { get; private set; }

        private readonly List<ClaimStatusHistory> _statusHistory = new();
        public IReadOnlyCollection<ClaimStatusHistory> StatusHistory =>
            _statusHistory.AsReadOnly();
        private Claim() { }
        public static Claim Register(
        Guid policyId,
        ClaimType type,
        decimal claimedAmount,
        DateOnly incidentDate,
        string description,
        DateOnly policyStartDate,
        DateOnly policyEndDate,
        decimal availableInsuredAmount,
        int currentOpenClaimsCount,
        string responsibleUser,
        Guid? createdByAdvisorId = null)
        {
            if (currentOpenClaimsCount >= MaxOpenClaimsPerPolicy)
                throw new ClaimLimitExceededException(policyId);

            if(incidentDate < policyStartDate || incidentDate > policyEndDate)
                throw new ClaimOnExpiredPolicyException(policyId, incidentDate);

            if (claimedAmount > availableInsuredAmount)
                throw new BusinessRuleException("AMOUNT_EXCEEDS_COVERAGE",
                    $"Claimed amount ${claimedAmount} exceeds available insured amount ${availableInsuredAmount}.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Claim description is required.", nameof(description));

            var claim = new Claim
            {
                PolicyId = policyId,
                Type = type,
                ClaimedAmount = claimedAmount,
                IncidentDate = incidentDate,
                Description = description.Trim(),
                Status = ClaimStatus.PendingApproval,
                HasBeenAppealed = false,
                CreatedByAdvisorId = createdByAdvisorId,
            };

            claim.AddStatusHistory(ClaimStatus.PendingApproval, responsibleUser, "Claim pending leader approval.");
            claim.AddDomainEvent(new ClaimRegisteredEvent(
                claim.Id, policyId, string.Empty, claimedAmount));

            return claim;
        }

        /// <summary>Leader approves the claim — moves from PendingApproval into the normal workflow (Registered).</summary>
        public void ApprovePending(string responsibleUser, string? observations = null)
        {
            if (Status != ClaimStatus.PendingApproval)
                throw new InvalidClaimStateException(Status.ToString(), nameof(ApprovePending));

            var previous = Status;
            Status = ClaimStatus.Registered;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Registered, responsibleUser, observations ?? "Claim approved by leader.");
            AddDomainEvent(new ClaimStatusChangedEvent(Id, PolicyId, previous, Status, responsibleUser, observations));
        }

        /// <summary>Leader rejects the claim — moves from PendingApproval to Rejected.</summary>
        public void RejectPending(string reason, string responsibleUser)
        {
            if (Status != ClaimStatus.PendingApproval)
                throw new InvalidClaimStateException(Status.ToString(), nameof(RejectPending));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.", nameof(reason));

            var previous = Status;
            Status = ClaimStatus.Rejected;
            RejectionReason = reason;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Rejected, responsibleUser, reason);
            AddDomainEvent(new ClaimStatusChangedEvent(Id, PolicyId, previous, Status, responsibleUser, reason));
        }

        public void StartInvestigation(string responsibleUser, string? observations = null)
        {
            if (Status != ClaimStatus.Registered)
                throw new InvalidClaimStateException(Status.ToString(), nameof(StartInvestigation));

            var previous = Status;
            Status = ClaimStatus.UnderInvestigation;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.UnderInvestigation, responsibleUser, observations);
            AddDomainEvent(new ClaimStatusChangedEvent(Id, PolicyId, previous, Status, responsibleUser, observations));
        }

        public void Approve(decimal approvedAmount, string responsibleUser, string? observations = null)
        {
            if(Status != ClaimStatus.UnderInvestigation && Status != ClaimStatus.Appealed)
                throw new InvalidClaimStateException(Status.ToString(), nameof (Approve));

            if (approvedAmount <= 0 || approvedAmount > ClaimedAmount)
                throw new ArgumentException($"Approved amount must be between $0 and claimed amount ${ClaimedAmount}.",
                nameof(approvedAmount));

            var previous = Status;
            Status = ClaimStatus.Approved;
            ApprovedAmount = approvedAmount;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Approved, responsibleUser, observations);
            AddDomainEvent(new ClaimStatusChangedEvent(
                Id, PolicyId, previous, Status, responsibleUser, observations));
        }

        public void Appeal(string responsibleUser, string? observations = null)
        {
            if (Status != ClaimStatus.Rejected)
                throw new InvalidClaimStateException(Status.ToString(), nameof(Appeal));

            if (HasBeenAppealed)
                throw new BusinessRuleException("APPEAL_ALREADY_USED",
                    $"Claim '{Id}' has already been appealed once.");

            var previous = Status;
            Status = ClaimStatus.Appealed;
            HasBeenAppealed = true;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Appealed, responsibleUser, observations);
            AddDomainEvent(new ClaimStatusChangedEvent(
                Id, PolicyId, previous, Status, responsibleUser, observations));

        }

        public void Reject(string reason, string responsibleUser)
        {
            if (Status != ClaimStatus.UnderInvestigation &&
                Status != ClaimStatus.Appealed)
                throw new InvalidClaimStateException(Status.ToString(), nameof(Reject));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.", nameof(reason));

            var previous = Status;
            Status = ClaimStatus.Rejected;
            RejectionReason = reason;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Rejected, responsibleUser, reason);
            AddDomainEvent(new ClaimStatusChangedEvent(
                Id, PolicyId, previous, Status, responsibleUser, reason));
        }
        public void RegisterPayment(string responsibleUser, string? observations = null)
        {
            if (Status != ClaimStatus.Approved)
                throw new InvalidClaimStateException(Status.ToString(), nameof(RegisterPayment));

            var previous = Status;
            Status = ClaimStatus.Paid;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Paid, responsibleUser, observations);
            AddDomainEvent(new ClaimStatusChangedEvent(
                Id, PolicyId, previous, Status, responsibleUser, observations));
        }

        public bool IsOpen() => Status is
        ClaimStatus.PendingApproval or
        ClaimStatus.Registered or
        ClaimStatus.UnderInvestigation or
        ClaimStatus.Appealed;

        private void AddStatusHistory(ClaimStatus status, string responsibleUser, string? observations) =>
               _statusHistory.Add(ClaimStatusHistory.Create(Id, status, responsibleUser, observations));
    }
}

