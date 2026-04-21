package com.insuratech.application.claims.commands;

import jakarta.validation.constraints.NotBlank;

public record RejectPendingClaimCommand(String claimId, @NotBlank String reason, String responsibleUser) {}
