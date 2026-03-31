using InsuraTech.Domain.Notifications;

namespace InsuraTech.Application.Notifications.DTOs;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification n) => new(
        n.Id,
        n.RecipientId,
        n.RecipientName,
        n.Subject,
        n.Body,
        n.Type,
        n.Status,
        n.SentAt,
        n.FailureReason,
        n.CorrelationId,
        n.CreatedAt);
}
