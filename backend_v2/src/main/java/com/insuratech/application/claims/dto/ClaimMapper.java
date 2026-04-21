package com.insuratech.application.claims.dto;

import com.insuratech.domain.claims.Claim;

public final class ClaimMapper {

    private ClaimMapper() {}

    public static ClaimResponse toResponse(Claim claim) {
        return new ClaimResponse(
            claim.getId(),
            claim.getPolicyId(),
            claim.getClaimType().name(),
            claim.getStatus().name(),
            claim.getDescription(),
            claim.getClaimedAmount(),
            claim.getApprovedAmount(),
            claim.getIncidentDate() != null
                ? java.time.LocalDate.ofInstant(claim.getIncidentDate(),
                    java.time.ZoneOffset.UTC) : null,
            claim.getReportedBy(),
            claim.getRejectionReason(),
            claim.getCreatedAt(),
            claim.getUpdatedAt()
        );
    }
}
