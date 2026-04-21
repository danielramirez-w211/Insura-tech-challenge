package com.insuratech.application.vehicleplans.dto;

import java.math.BigDecimal;
import java.util.List;

public record VehiclePlanOptionDto(
    String planId,
    String planName,
    BigDecimal monthlyPremium,
    BigDecimal annualPremiumWithDiscount,
    List<String> coverages,
    List<String> assistances
) {}
