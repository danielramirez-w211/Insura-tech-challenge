package com.insuratech.domain.common;

import lombok.Getter;

import java.time.Instant;

@Getter
public class PolicyStatusHistory {

    private final String status;
    private final Instant changedAt;
    private final String changedBy;
    private final String reason;

    public PolicyStatusHistory(String status, String changedBy, String reason) {
        this.status = status;
        this.changedAt = Instant.now();
        this.changedBy = changedBy;
        this.reason = reason;
    }
}
