using InsuraTech.Application.Common.Models;
using InsuraTech.Application.Notifications.DTOs;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsHandler
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<PagedResult<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _notificationRepository.GetAllAsync(
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(n => n.ToDto());
        return new PagedResult<NotificationDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
