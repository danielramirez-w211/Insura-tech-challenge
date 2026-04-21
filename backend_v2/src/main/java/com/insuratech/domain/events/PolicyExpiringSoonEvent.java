package com.insuratech.domain.events;

import com.insuratech.domain.common.IDomainEvent;

import java.time.Instant;
import java.time.LocalDate;

public record PolicyExpiringSoonEvent(String policyId, String policyNumber, LocalDate expirationDate, Instant occurredOn) implements IDomainEvent {
    public PolicyExpiringSoonEvent(String policyId, String policyNumber, LocalDate expirationDate) {
        this(policyId, policyNumber, expirationDate, Instant.now());
    }
}
