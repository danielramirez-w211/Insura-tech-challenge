package com.insuratech.domain.events;

import com.insuratech.domain.common.IDomainEvent;

import java.time.Instant;

public record PolicyCancelledEvent(String policyId, String reason, Instant occurredOn) implements IDomainEvent {
    public PolicyCancelledEvent(String policyId, String reason) {
        this(policyId, reason, Instant.now());
    }
}
