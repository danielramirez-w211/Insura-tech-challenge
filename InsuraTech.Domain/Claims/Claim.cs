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
        string responsibleUser)
        {
            if (currentOpenClaimsCount >= MaxOpenClaimsPerPolicy)
                throw new ClaimLimitExceededException(policyId);

            if(incidentDate < policyStartDate || incidentDate > policyEndDate)
                throw new ClaimOnExpiredPolicyException(policyId, incidentDate);

            if(claimedAmount <= 0)
                throw new ArgumentException("AMOUNT_EXCEEDS_COVERAGE",
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
                Status = ClaimStatus.Registered,
                HasBeenAppealed = false,
            };

            claim.AddStatusHistory(ClaimStatus.Registered, responsibleUser, "Claim resgistered. ");
            claim.AddDomainEvent(new ClaimRegisteredEvent(
                claim.Id, policyId, string.Empty, claimedAmount));

            return claim;
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
            if (Status != ClaimStatus.Registered)
                throw new InvalidClaimStateException(Status.ToString(), nameof(Appeal));
            if (HasBeenAppealed)
                throw new BusinessRuleException("APPEAL_ALREADY_USED",
                $"Claim '{Id}' has already been appealed once. No further appeals are allowed.");

            var previous = Status;
            Status = ClaimStatus.Approved;
            HasBeenAppealed = true;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(ClaimStatus.Approved, responsibleUser, observations);
            AddDomainEvent(new ClaimStatusChangedEvent(
                Id, PolicyId, previous, Status, responsibleUser, observations));

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
        ClaimStatus.Registered or
        ClaimStatus.UnderInvestigation or
        ClaimStatus.Appealed;

        private void AddStatusHistory(ClaimStatus status, string responsibleUser, string? observations) =>
               _statusHistory.Add(ClaimStatusHistory.Create(Id, status, responsibleUser, observations));
    }
}

