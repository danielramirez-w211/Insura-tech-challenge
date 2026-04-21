package com.insuratech.application.claims.commands;

public record RejectClaimCommand(String claimId, String reason, String rejectedBy) {}
