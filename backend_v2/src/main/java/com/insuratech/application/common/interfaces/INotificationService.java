package com.insuratech.application.common.interfaces;

public interface INotificationService {
    void send(String recipientUserId, String subject, String body);
}
