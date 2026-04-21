package com.insuratech.application.notifications.dto;
import java.time.Instant;
public record NotificationDto(
    String id, String recipientUserId, String type, String status,
    String title, String body, String relatedEntityId, Instant createdAt
) {}
