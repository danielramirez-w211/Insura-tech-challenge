package com.insuratech.domain.claims;

import lombok.Getter;

import java.time.Instant;

@Getter
public class ClaimStatusHistory {

    private final ClaimStatus status;
    private final Instant changedAt;
    private final String changedBy;
    private final String notes;

    public ClaimStatusHistory(ClaimStatus status, String changedBy, String notes) {
        this.status = status;
        this.changedAt = Instant.now();
        this.changedBy = changedBy;
        this.notes = notes;
    }
}
