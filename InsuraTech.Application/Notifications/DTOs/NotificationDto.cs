using InsuraTech.Domain.Notifications;

namespace InsuraTech.Application.Notifications.DTOs;

public sealed record NotificationDto(
    Guid Id,
    Guid RecipientId,
    string RecipientName,
    string Subject,
    string Body,
    NotificationType Type,
    NotificationStatus Status,
    DateTime? SentAt,
    string? FailureReason,
    string? CorrelationId,
    DateTime CreatedAt);
