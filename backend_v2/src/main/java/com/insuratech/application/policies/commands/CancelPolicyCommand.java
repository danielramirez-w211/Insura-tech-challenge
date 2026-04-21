package com.insuratech.application.policies.commands;

import jakarta.validation.constraints.NotBlank;

public record CancelPolicyCommand(
    @NotBlank String policyId,
    @NotBlank String reason,
    @NotBlank String cancelledBy
) {}
