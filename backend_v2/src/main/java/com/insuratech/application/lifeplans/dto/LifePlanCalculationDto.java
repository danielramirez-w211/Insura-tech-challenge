package com.insuratech.application.lifeplans.dto;

import java.math.BigDecimal;

public record LifePlanCalculationDto(
    String planId,
    String planName,
    int insuredAge,
    BigDecimal annualPremium,
    BigDecimal monthlyPremium,
    int durationDays,
    BigDecimal deathBenefit,
    BigDecimal funeralExpenses,
    BigDecimal burialExpenses,
    BigDecimal beneficiaryCompensation
) {}
