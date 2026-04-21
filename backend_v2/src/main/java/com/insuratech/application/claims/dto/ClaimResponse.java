package com.insuratech.application.claims.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.time.LocalDate;

public record ClaimResponse(
    String id,
    String policyId,
    String type,
    String status,
    String description,
    BigDecimal claimedAmount,
    BigDecimal approvedAmount,
    LocalDate incidentDate,
    String reportedBy,
    String rejectionReason,
    Instant createdAt,
    Instant updatedAt
) {}
