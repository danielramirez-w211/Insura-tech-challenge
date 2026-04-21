package com.insuratech.application.homeplans.dto;

import java.math.BigDecimal;
import java.util.List;

public record HomeQuotationDto(
    BigDecimal propertyValue,
    int constructionYear,
    int propertyAge,
    int stratum,
    int occupants,
    String propertyType,
    BigDecimal baseMonthlyPremium,
    List<String> selectedCoverages,
    List<String> appliedMultipliers,
    BigDecimal finalMonthlyPremium
) {}
