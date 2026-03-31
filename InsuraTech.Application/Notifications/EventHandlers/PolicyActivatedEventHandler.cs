using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using MediatR;

namespace InsuraTech.Application.Notifications.EventHandlers;

public sealed class PolicyActivatedEventHandler : INotificationHandler<PolicyActivatedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PolicyActivatedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PolicyActivatedEvent notification, CancellationToken cancellationToken)
    {
        var body = $"Your policy {notification.PolicyNumber} has been activated. " +
                   $"Insured: {notification.InsuredFullNamed} ({notification.InsuredDocumentId}).";

        var notif = Notification.Create(
            notification.PolicyId,
            notification.InsuredFullNamed,
            "Policy Activated",
            body,
            NotificationType.PolicyActivated);

        await _notificationRepository.AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
