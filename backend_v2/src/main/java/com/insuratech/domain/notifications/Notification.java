package com.insuratech.domain.notifications;

import com.insuratech.domain.common.AggregateRoot;
import lombok.Getter;
import org.springframework.data.mongodb.core.mapping.Document;

@Getter
@Document(collection = "notifications")
public class Notification extends AggregateRoot {

    private String recipientUserId;
    private NotificationType type;
    private NotificationStatus status;
    private String title;
    private String body;
    private String relatedEntityId;

    protected Notification() { super(); }

    public static Notification create(String recipientUserId, NotificationType type,
                                      String title, String body, String relatedEntityId) {
        Notification n = new Notification();
        n.recipientUserId = recipientUserId;
        n.type = type;
        n.status = NotificationStatus.PENDING;
        n.title = title;
        n.body = body;
        n.relatedEntityId = relatedEntityId;
        return n;
    }

    public void markAsSent() {
        this.status = NotificationStatus.SENT;
        markAsUpdated();
    }

    public void markAsFailed() {
        this.status = NotificationStatus.FAILED;
        markAsUpdated();
    }

    public void markAsRead() {
        this.status = NotificationStatus.READ;
        markAsUpdated();
    }
}
