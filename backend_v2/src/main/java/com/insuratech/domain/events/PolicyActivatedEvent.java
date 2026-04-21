package com.insuratech.domain.events;

import com.insuratech.domain.common.IDomainEvent;

import java.time.Instant;

public record PolicyActivatedEvent(String policyId, String policyNumber, Instant occurredOn) implements IDomainEvent {
    public PolicyActivatedEvent(String policyId, String policyNumber) {
        this(policyId, policyNumber, Instant.now());
    }
}
