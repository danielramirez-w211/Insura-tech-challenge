package com.insuratech.application.claims.commands;

import jakarta.validation.constraints.NotBlank;

public record AppealClaimCommand(
    String claimId,
    @NotBlank String reason,
    String appealedBy
) {}
