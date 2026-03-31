using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Notifications;

public sealed class Notification : Entity
{
    public Guid RecipientId { get; private set; }
    public string RecipientName { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public NotificationType Type { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? CorrelationId { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid recipientId,
        string recipientName,
        string subject,
        string body,
        NotificationType type,
        string? correlationId = null)
    {
        return new Notification
        {
            RecipientId = recipientId,
            RecipientName = recipientName,
            Subject = subject,
            Body = body,
            Type = type,
            Status = NotificationStatus.Pending,
            CorrelationId = correlationId
        };
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
        MarkAsUpdated();
    }

    public void Retry()
    {
        if (Status != NotificationStatus.Failed)
            throw new InvalidOperationException(
                $"Only failed notifications can be retried. Current status: {Status}.");

        Status = NotificationStatus.Pending;
        FailureReason = null;
        SentAt = null;
        MarkAsUpdated();
    }
}
