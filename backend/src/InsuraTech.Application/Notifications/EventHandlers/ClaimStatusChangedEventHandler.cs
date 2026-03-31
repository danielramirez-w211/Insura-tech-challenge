using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using MediatR;

namespace InsuraTech.Application.Notifications.EventHandlers;

public sealed class ClaimStatusChangedEventHandler : INotificationHandler<ClaimStatusChangedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimStatusChangedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ClaimStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var body = $"Claim {notification.ClaimId} status changed from " +
                   $"{notification.PreviousStatus} to {notification.NewStatus}. " +
                   $"Responsible: {notification.ResponsibleUser}." +
                   (notification.Observations is not null ? $" Notes: {notification.Observations}" : string.Empty);

        var notif = Notification.Create(
            notification.PolicyId,
            notification.ResponsibleUser,
            $"Claim Status Changed: {notification.NewStatus}",
            body,
            NotificationType.ClaimStatusChanged);

        await _notificationRepository.AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
