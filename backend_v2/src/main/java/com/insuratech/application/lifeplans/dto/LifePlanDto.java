package com.insuratech.application.lifeplans.dto;

import java.math.BigDecimal;

public record LifePlanDto(
    String planId,
    String planName,
    BigDecimal annualPremium,
    BigDecimal monthlyPremium,
    BigDecimal deathBenefit,
    BigDecimal funeralExpenses,
    BigDecimal burialExpenses,
    BigDecimal beneficiaryCompensation
) {}
