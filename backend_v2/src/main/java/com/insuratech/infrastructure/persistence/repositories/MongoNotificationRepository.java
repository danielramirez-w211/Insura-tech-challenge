// Origen: NotificationRepository.cs → MongoNotificationRepository.java
package com.insuratech.infrastructure.persistence.repositories;

import com.insuratech.domain.notifications.Notification;
import com.insuratech.domain.notifications.NotificationStatus;
import com.insuratech.domain.ports.NotificationRepository;
import com.insuratech.infrastructure.persistence.springdata.NotificationSpringRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;

@Component
@RequiredArgsConstructor
public class MongoNotificationRepository implements NotificationRepository {

    private final NotificationSpringRepository springRepo;

    @Override
    public Notification save(Notification notification) { return springRepo.save(notification); }

    @Override
    public Optional<Notification> findById(String id) {
        return springRepo.findById(id).filter(n -> !n.isDeleted());
    }

    @Override
    public List<Notification> findByRecipientUserId(String userId) {
        return springRepo.findByRecipientUserIdAndDeletedFalse(userId);
    }

    @Override
    public List<Notification> findByRecipientUserIdAndStatus(String userId, NotificationStatus status) {
        return springRepo.findByRecipientUserIdAndStatusAndDeletedFalse(userId, status);
    }

    @Override
    public long countUnreadByUserId(String userId) {
        return springRepo.countByRecipientUserIdAndStatusAndDeletedFalse(userId, NotificationStatus.PENDING);
    }

    @Override
    public void deleteById(String id) {
        findById(id).ifPresent(n -> {
            n.setDeleted(true);
            springRepo.save(n);
        });
    }
}
