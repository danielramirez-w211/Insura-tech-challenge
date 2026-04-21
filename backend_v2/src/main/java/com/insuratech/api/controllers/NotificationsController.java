// Spec: SPEC-015 — notificaciones
package com.insuratech.api.controllers;

import com.insuratech.application.notifications.NotificationService;
import com.insuratech.application.notifications.commands.RetryNotificationCommand;
import com.insuratech.application.notifications.dto.NotificationDto;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/v1/notifications")
@RequiredArgsConstructor
public class NotificationsController {

    private final NotificationService notificationService;

    private String currentUserId() {
        return (String) SecurityContextHolder.getContext().getAuthentication().getPrincipal();
    }

    @GetMapping
    public ResponseEntity<List<NotificationDto>> getAll() {
        return ResponseEntity.ok(notificationService.getByUser(currentUserId()));
    }

    @PutMapping("/{id}/retry")
    public ResponseEntity<NotificationDto> retry(@PathVariable String id) {
        return ResponseEntity.ok(notificationService.retry(new RetryNotificationCommand(id)));
    }
}
