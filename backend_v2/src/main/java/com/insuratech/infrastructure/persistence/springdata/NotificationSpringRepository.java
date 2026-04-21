package com.insuratech.infrastructure.persistence.springdata;

import com.insuratech.domain.notifications.Notification;
import com.insuratech.domain.notifications.NotificationStatus;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface NotificationSpringRepository extends MongoRepository<Notification, String> {
    List<Notification> findByRecipientUserIdAndDeletedFalse(String userId);
    List<Notification> findByRecipientUserIdAndStatusAndDeletedFalse(String userId, NotificationStatus status);
    long countByRecipientUserIdAndStatusAndDeletedFalse(String userId, NotificationStatus status);
}
