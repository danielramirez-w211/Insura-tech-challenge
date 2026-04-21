package com.insuratech.application.healthplans.dto;

import java.math.BigDecimal;

public record HealthPlanCalculationDto(
    String planId,
    String planName,
    BigDecimal baseAmount,
    int ageFactorPercentage,
    BigDecimal ageFactorAmount,
    BigDecimal finalAmount,
    int insuredAge,
    BigDecimal monthlyPremium,
    int durationDays
) {}
