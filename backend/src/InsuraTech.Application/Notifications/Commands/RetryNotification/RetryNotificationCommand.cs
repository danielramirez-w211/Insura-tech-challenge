using InsuraTech.Application.Notifications.DTOs;
using MediatR;

namespace InsuraTech.Application.Notifications.Commands.RetryNotification;

public sealed record RetryNotificationCommand(Guid NotificationId) : IRequest<NotificationDto>;
