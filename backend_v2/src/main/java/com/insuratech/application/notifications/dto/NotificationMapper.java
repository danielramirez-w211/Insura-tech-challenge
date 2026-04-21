package com.insuratech.application.notifications.dto;
import com.insuratech.domain.notifications.Notification;
public final class NotificationMapper {
    private NotificationMapper() {}
    public static NotificationDto toDto(Notification n) {
        return new NotificationDto(n.getId(), n.getRecipientUserId(),
            n.getType().name(), n.getStatus().name(),
            n.getTitle(), n.getBody(), n.getRelatedEntityId(), n.getCreatedAt());
    }
}
