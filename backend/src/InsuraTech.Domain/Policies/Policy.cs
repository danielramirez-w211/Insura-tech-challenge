using InsuraTech.Domain.Common;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Policies.HealthPlan;
using InsuraTech.Domain.Policies.TravelPlan;
using InsuraTech.Domain.Policies.ValueObjects;

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
        public HealthPlanSelection? HealthPlan { get; private set; }
        public TravelPlanSelection? TravelPlan { get; private set; }

        private readonly List<PolicyStatusHistory> _statusHistory = new();
        public IReadOnlyCollection<PolicyStatusHistory> StatusHistory =>
            _statusHistory.AsReadOnly();

        private Policy() { }

        // Factory — tipos con monto manual (Life transitorio; Vehicle y Home en futuras specs)
        public static Policy Create(
            PolicyNumber number,
            PolicyType type,
            InsuredPerson insured,
            CoveragePeriod coverage,
            decimal monthlyPremium,
            decimal insuredAmount)
        {
            if (insuredAmount <= 0)
                throw new ArgumentException("Insured amount must be greater than zero.", nameof(insuredAmount));
            if (monthlyPremium <= 0)
                throw new ArgumentException("Monthly premium must be greater than zero.", nameof(monthlyPremium));

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

        // Factory — tipo Health (plan predefinido + cálculo automático)
        public static Policy CreateHealthPolicy(
            PolicyNumber number,
            InsuredPerson insured,
            CoveragePeriod coverage,
            decimal monthlyPremium,
            string healthPlanId,
            DateOnly today)
        {
            var selection = HealthPlanPricingService.Calculate(healthPlanId, insured.BirthDate, today);

            if (monthlyPremium <= 0)
                throw new ArgumentException("Monthly premium must be greater than zero.", nameof(monthlyPremium));

            var policy = new Policy
            {
                Number = number,
                Type = PolicyType.Health,
                Insured = insured,
                Coverage = coverage,
                MonthlyPremium = monthlyPremium,
                InsuredAmount = selection.FinalAmount,
                AvailableInsuredAmount = selection.FinalAmount,
                HealthPlan = selection,
                Status = PolicyStatus.Pending
            };

            policy.AddStatusHistory(PolicyStatus.Pending,
                $"Health policy created with plan '{selection.PlanName}'.");
            return policy;
        }

        // Factory — tipo Travel (rating engine automático)
        public static Policy CreateTravelPolicy(
            PolicyNumber number,
            InsuredPerson insured,
            CoveragePeriod coverage,
            TripType tripType,
            Continent? continent,
            int durationDays,
            decimal? trmCop,
            DateOnly? trmDate)
        {
            var selection = TravelRatingService.Calculate(
                tripType, continent, durationDays, trmCop, trmDate, DateTime.UtcNow);

            var policy = new Policy
            {
                Number                  = number,
                Type                    = PolicyType.Travel,
                Insured                 = insured,
                Coverage                = coverage,
                MonthlyPremium          = selection.TotalPriceCop,
                InsuredAmount           = selection.TotalPriceCop,
                AvailableInsuredAmount  = selection.TotalPriceCop,
                TravelPlan              = selection,
                Status                  = PolicyStatus.Pending
            };

            policy.AddStatusHistory(PolicyStatus.Pending,
                $"Travel policy created. Type: {tripType}" +
                (continent.HasValue ? $", Continent: {continent}" : string.Empty) +
                $", Duration: {durationDays} days, Total: ${selection.TotalPriceCop:N0} COP.");

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
                Id, Number.Value, Insured.FirstName, Insured.DocumentId));
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




        private void AddStatusHistory(PolicyStatus status, string notes) =>
                _statusHistory.Add(PolicyStatusHistory.Create(Id, status, notes));
    }
}
