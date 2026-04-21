package com.insuratech.application.policies.dto;

import java.math.BigDecimal;

public record HealthPlanSelectionDto(
    String planId, String planName,
    BigDecimal basePremium, BigDecimal calculatedPremium
) {}
