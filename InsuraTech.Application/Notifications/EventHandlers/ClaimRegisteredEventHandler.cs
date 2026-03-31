using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Events;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using MediatR;

namespace InsuraTech.Application.Notifications.EventHandlers;

public sealed class ClaimRegisteredEventHandler : INotificationHandler<ClaimRegisteredEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimRegisteredEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ClaimRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var body = $"Your claim has been registered for policy {notification.PolicyNumer}. " +
                   $"Claimed amount: ${notification.ClaimAmount:N2}. " +
                   $"Claim ID: {notification.ClaimId}.";

        var notif = Notification.Create(
            notification.PolicyId,
            string.Empty,
            "Claim Registered",
            body,
            NotificationType.ClaimRegistered);

        await _notificationRepository.AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
