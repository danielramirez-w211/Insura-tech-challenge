package com.insuratech.application.claims.commands;

import com.insuratech.domain.claims.ClaimType;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;
import java.math.BigDecimal;
import java.time.LocalDate;

public record RegisterClaimCommand(
    @NotBlank String policyId,
    @NotNull ClaimType type,
    @NotNull @Positive BigDecimal claimedAmount,
    @NotNull LocalDate incidentDate,
    @NotBlank String description,
    @NotBlank String responsibleUser,
    String createdByAdvisorId
) {}
