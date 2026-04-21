package com.insuratech.domain.ports;

import com.insuratech.domain.notifications.Notification;
import com.insuratech.domain.notifications.NotificationStatus;

import java.util.List;
import java.util.Optional;

public interface NotificationRepository {

    Notification save(Notification notification);

    Optional<Notification> findById(String id);

    List<Notification> findByRecipientUserId(String userId);

    List<Notification> findByRecipientUserIdAndStatus(String userId, NotificationStatus status);

    long countUnreadByUserId(String userId);

    void deleteById(String id);
}
