package com.insuratech.application.claims.commands;

public record ApprovePendingClaimCommand(String claimId, String responsibleUser) {}
