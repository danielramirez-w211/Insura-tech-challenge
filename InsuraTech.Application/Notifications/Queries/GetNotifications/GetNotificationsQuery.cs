using InsuraTech.Application.Common.Models;
using InsuraTech.Application.Notifications.DTOs;
using InsuraTech.Domain.Notifications;
using MediatR;

namespace InsuraTech.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public NotificationStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
