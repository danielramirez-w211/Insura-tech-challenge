using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.ValueObjects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsuraTech.Domain.Policies
{
    public sealed class Policy : AggregateRoot
    {
        public PolicyNumber Number { get; private set; } = null!;
        public PolicyType Type { get; private set; }
        public PolicyStatus Status { get; private set; }
        public InsuredPerson Insured { get; private set; } = null!;
        public CoveragePeriod Coverage { get; private set; } = null!;
        public decimal MonthlyPremium { get; private set; }
        public decimal InsuredAmount { get; private set; }
        public decimal AvailableInsuredAmount { get; private set; }
        public string? CancellationReason { get; private set; }
        public DateOnly? CancellationEffectiveDate { get; private set; }
        public Guid? RenewedFromPolicyId { get; private set; }

        private readonly List<PolicyStatusHistory> _statusHistory = new();
        public IReadOnlyCollection<PolicyStatusHistory> StatusHistory =>
            _statusHistory.AsReadOnly();

        private Policy() { }

        // Factory 

        public static Policy Create(
            PolicyNumber number,
            PolicyType type,
            InsuredPerson insured,
            CoveragePeriod coverage,
            decimal monthlyPremium,
            decimal insuredAmount)
        {
            ValidateFinancials(monthlyPremium, insuredAmount);

            var policy = new Policy
            {
                Number = number,
                Type = type,
                Insured = insured,
                Coverage = coverage,
                MonthlyPremium = monthlyPremium,
                InsuredAmount = insuredAmount,
                AvailableInsuredAmount = insuredAmount,
                Status = PolicyStatus.Pending
            };

            policy.AddStatusHistory(PolicyStatus.Pending, "Policy created.");
            return policy;
        }

        // Poliza Activa
        public void Activate()
        {
            if (Status != PolicyStatus.Pending)
                throw new InvalidPolicyStateException(Status.ToString(), nameof(Activate));

            Status = PolicyStatus.Active;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(PolicyStatus.Active, "Payment confirmed. Policy activated.");
            AddDomainEvent(new PolicyActivatedEvent(
                Id, Number.Value, Insured.FullName, Insured.DocumentId));
        }
        // Poliza suspendida

        public void Suspend(string reason)
        {
            if (Status != PolicyStatus.Active)
                throw new InvalidPolicyStateException(Status.ToString(), nameof(Suspend));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Suspenson reason is requiered. ", nameof(reason));
            Status = PolicyStatus.Suspended;
            MarkAsUpdated();
            IncrementVersion();
            AddStatusHistory(PolicyStatus.Suspended, reason);
        }

        //Poliza cancelada 
        public void Cancel(string reason, DateOnly effectiveDate)
        {
            if (Status == PolicyStatus.Cancelled)
                throw new InvalidPolicyStateException(Status.ToString(), nameof(Cancel));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Cancellation reason is required.", nameof(reason));

            if (effectiveDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Effective date cannot be in the past.", nameof(effectiveDate));

            Status = PolicyStatus.Cancelled;
            CancellationReason = reason;
            CancellationEffectiveDate = effectiveDate;
            MarkAsUpdated();
            IncrementVersion();

            AddStatusHistory(PolicyStatus.Cancelled, $"Cancelled: {reason}");
            AddDomainEvent(new PolicyCancelledEvent(Id, Number.Value, reason, effectiveDate));

        }
        // Poliza renovacion

        public Policy Renew(PolicyNumber newNumber, CoveragePeriod newCoverage)
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Expired)
                throw new InvalidPolicyStateException(Status.ToString(), nameof(Renew));

            var renewal = new Policy
            {
                Number = newNumber,
                Type = Type,
                Insured = Insured,
                Coverage = newCoverage,
                MonthlyPremium = MonthlyPremium,
                InsuredAmount = InsuredAmount,
                AvailableInsuredAmount = InsuredAmount,
                Status = PolicyStatus.Pending,
                RenewedFromPolicyId = Id
            };

            renewal.AddStatusHistory(PolicyStatus.Pending, $"Renewed from policy {Number.Value}.");
            return renewal;
        }
        // Monto asegurado deducible cuando hay siniestros

        public void DeductInsuredAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to deduct must be greater than zero.", nameof(amount));
            if (amount > AvailableInsuredAmount)
                throw new BusinessRuleException("INSUFFICIENT_COVERAGE",
                    $"Claim amount ${amount} exceeds available insured amount ${AvailableInsuredAmount}.");
            AvailableInsuredAmount -= amount;
            MarkAsUpdated();
            IncrementVersion();

            if(AvailableInsuredAmount == 0)
            {
                Status = PolicyStatus.Exhausted;
                AddStatusHistory(PolicyStatus.Exhausted, "Insured amount fully exhausted by approved claims.");
            }
        }

        public bool IsActiveOn(DateOnly date) =>
            Status == PolicyStatus.Active && Coverage.IsActive(date);


        private static void ValidateFinancials(decimal monthlyPremium, decimal insuredAmount)
        {
            if (insuredAmount <= 0)
                throw new ArgumentException("Insured amount must be greater than zero.", nameof(insuredAmount));

            if (monthlyPremium <= 0)
                throw new ArgumentException("Monthly premium must be greater than zero.", nameof(monthlyPremium));

            var maxPremium = insuredAmount * 0.05m;
            if (monthlyPremium > maxPremium)
                throw new ArgumentException(
                    $"Monthly premium cannot exceed 5% of insured amount (max: ${maxPremium:F2}).",
                    nameof(monthlyPremium));
        }

        private void AddStatusHistory(PolicyStatus status, string notes) =>
                _statusHistory.Add(PolicyStatusHistory.Create(Id, status, notes));
    }
}
