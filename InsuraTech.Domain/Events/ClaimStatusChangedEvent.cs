namespace InsuraTech.Domain.Events;

using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;

public sealed class ClaimStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public Guid Id => EventId;

    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(ClaimStatusChangedEvent);

    public Guid ClaimId { get; }
    public Guid PolicyId { get; }
    public ClaimStatus PreviousStatus { get; }
    public ClaimStatus NewStatus { get; }
    public string ResponsibleUser { get; }
    public string? Observations { get; }

    public ClaimStatusChangedEvent(
        Guid claimId,
        Guid policyId,
        ClaimStatus previousStatus,
        ClaimStatus newStatus,
        string responsibleUser,
        string? observations = null)
    {
        ClaimId = claimId;
        PolicyId = policyId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ResponsibleUser = responsibleUser;
        Observations = observations;
    }
}