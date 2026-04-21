// Spec: SPEC-015 — notifications
// Origen: GetNotificationsHandler, RetryNotificationHandler → NotificationService.java
package com.insuratech.application.notifications;

import com.insuratech.application.common.exceptions.NotFoundException;
import com.insuratech.application.common.interfaces.INotificationService;
import com.insuratech.application.notifications.commands.RetryNotificationCommand;
import com.insuratech.application.notifications.dto.NotificationDto;
import com.insuratech.application.notifications.dto.NotificationMapper;
import com.insuratech.domain.events.ClaimRegisteredEvent;
import com.insuratech.domain.events.PolicyActivatedEvent;
import com.insuratech.domain.notifications.Notification;
import com.insuratech.domain.notifications.NotificationStatus;
import com.insuratech.domain.notifications.NotificationType;
import com.insuratech.domain.ports.NotificationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Slf4j
@Service
@RequiredArgsConstructor
public class NotificationService {

    private final NotificationRepository notificationRepository;
    private final INotificationService externalNotificationService;

    public List<NotificationDto> getByUser(String userId) {
        return notificationRepository.findByRecipientUserId(userId)
            .stream().map(NotificationMapper::toDto).toList();
    }

    public List<NotificationDto> getUnreadByUser(String userId) {
        return notificationRepository
            .findByRecipientUserIdAndStatus(userId, NotificationStatus.PENDING)
            .stream().map(NotificationMapper::toDto).toList();
    }

    @Transactional
    public NotificationDto retry(RetryNotificationCommand cmd) {
        var notification = notificationRepository.findById(cmd.notificationId())
            .orElseThrow(() -> new NotFoundException("Notification", cmd.notificationId()));

        try {
            externalNotificationService.send(
                notification.getRecipientUserId(),
                notification.getTitle(),
                notification.getBody());
            notification.markAsSent();
        } catch (Exception e) {
            log.error("Failed to send notification id={}: {}", cmd.notificationId(), e.getMessage());
            notification.markAsFailed();
        }

        notificationRepository.save(notification);
        return NotificationMapper.toDto(notification);
    }

    @Transactional
    public void markAsRead(String notificationId) {
        var notification = notificationRepository.findById(notificationId)
            .orElseThrow(() -> new NotFoundException("Notification", notificationId));
        notification.markAsRead();
        notificationRepository.save(notification);
    }

    @EventListener
    public void onPolicyActivated(PolicyActivatedEvent event) {
        var notification = Notification.create(
            event.policyId(),
            NotificationType.POLICY_ACTIVATED,
            "Poliza activada",
            "Tu poliza " + event.policyNumber() + " ha sido activada exitosamente.",
            event.policyId()
        );
        notificationRepository.save(notification);
    }

    @EventListener
    public void onClaimRegistered(ClaimRegisteredEvent event) {
        var notification = Notification.create(
            event.policyId(),
            NotificationType.CLAIM_REGISTERED,
            "Reclamacion registrada",
            "Se registró una reclamación de tipo " + event.claimType() + " en tu póliza.",
            event.claimId()
        );
        notificationRepository.save(notification);
    }
}
