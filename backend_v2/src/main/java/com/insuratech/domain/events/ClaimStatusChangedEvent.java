package com.insuratech.domain.events;

import com.insuratech.domain.common.IDomainEvent;

import java.time.Instant;

public record ClaimStatusChangedEvent(String claimId, String previousStatus, String newStatus, Instant occurredOn) implements IDomainEvent {
    public ClaimStatusChangedEvent(String claimId, String previousStatus, String newStatus) {
        this(claimId, previousStatus, newStatus, Instant.now());
    }
}
