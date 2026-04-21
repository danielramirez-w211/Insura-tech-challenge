package com.insuratech.domain.events;

import com.insuratech.domain.common.IDomainEvent;

import java.time.Instant;

public record ClaimRegisteredEvent(String claimId, String policyId, String claimType, Instant occurredOn) implements IDomainEvent {
    public ClaimRegisteredEvent(String claimId, String policyId, String claimType) {
        this(claimId, policyId, claimType, Instant.now());
    }
}
