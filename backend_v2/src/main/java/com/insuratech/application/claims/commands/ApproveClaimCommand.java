package com.insuratech.application.claims.commands;

import java.math.BigDecimal;

public record ApproveClaimCommand(String claimId, BigDecimal approvedAmount, String approvedBy) {}
